using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;
using System.Collections.ObjectModel;

namespace PairEd123.ViewModels
{
    public partial class FeedbackViewModel : ObservableObject
    {
        private readonly FeedbackService _feedbackService;
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;

        [ObservableProperty] private bool isToReviewTabActive = true;
        [ObservableProperty] private bool isGivenTabActive;
        [ObservableProperty] private bool isReceivedTabActive;

        [ObservableProperty] private bool isReviewPromptVisible;
        [ObservableProperty] private int selectedRating;
        [ObservableProperty] private string reviewCommentText = string.Empty;
        private SessionToReview? _pendingReviewTarget;

        public ObservableCollection<SessionToReview> SessionsToReview { get; } = new();
        public ObservableCollection<FeedbackDisplay> GivenFeedback { get; } = new();
        public ObservableCollection<FeedbackDisplay> ReceivedFeedback { get; } = new();

        public FeedbackViewModel(FeedbackService feedbackService, CurrentUserService currentUserService)
        {
            _feedbackService = feedbackService;
            _currentUserService = currentUserService;
        }

        public async Task LoadAsync()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null)
            {
                StatusMessage = "No user logged in.";
                return;
            }
            IsBusy = true;
            try
            {
                var toReview = await _feedbackService.GetSessionsToReviewAsync(user.UserId);
                SessionsToReview.Clear();
                foreach (var s in toReview) SessionsToReview.Add(s);

                var given = await _feedbackService.GetGivenAsync(user.UserId);
                GivenFeedback.Clear();
                foreach (var f in given) GivenFeedback.Add(f);

                var received = await _feedbackService.GetReceivedAsync(user.UserId);
                ReceivedFeedback.Clear();
                foreach (var f in received) ReceivedFeedback.Add(f);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load feedback: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ShowToReviewTab()
        {
            IsToReviewTabActive = true;
            IsGivenTabActive = false;
            IsReceivedTabActive = false;
        }

        [RelayCommand]
        private void ShowGivenTab()
        {
            IsToReviewTabActive = false;
            IsGivenTabActive = true;
            IsReceivedTabActive = false;
        }

        [RelayCommand]
        private void ShowReceivedTab()
        {
            IsToReviewTabActive = false;
            IsGivenTabActive = false;
            IsReceivedTabActive = true;
        }

        [RelayCommand]
        private void StartReview(SessionToReview session)
        {
            if (session is null) return;
            _pendingReviewTarget = session;
            SelectedRating = 0;
            ReviewCommentText = string.Empty;
            IsReviewPromptVisible = true;
        }

        [RelayCommand]
        private void SetRating(string ratingValue)
        {
            if (int.TryParse(ratingValue, out int rating))
                SelectedRating = rating;
        }

        [RelayCommand]
        private async Task ConfirmReviewAsync()
        {
            var user = _currentUserService.CurrentUser;
            if (_pendingReviewTarget is null || user is null || IsBusy) return;
            if (SelectedRating < 1 || SelectedRating > 5)
            {
                StatusMessage = "Please select a rating.";
                return;
            }
            IsBusy = true;
            try
            {
                var (success, message) = await _feedbackService.SubmitFeedbackAsync(
                    _pendingReviewTarget.TutorId, _pendingReviewTarget.TuteeId, _pendingReviewTarget.SessionId,
                    user.UserId, SelectedRating, ReviewCommentText);

                if (success)
                {
                    SessionsToReview.Remove(_pendingReviewTarget);
                    IsReviewPromptVisible = false;
                    _pendingReviewTarget = null;
                    await LoadAsync(); // refresh Given tab with the new entry
                }
                else
                {
                    StatusMessage = message;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelReview()
        {
            IsReviewPromptVisible = false;
            _pendingReviewTarget = null;
            ReviewCommentText = string.Empty;
        }
    }
}