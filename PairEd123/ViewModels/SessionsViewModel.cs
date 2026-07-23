using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;

namespace PairEd123.ViewModels
{
    public partial class SessionsViewModel : ObservableObject
    {
        private readonly SessionsService _sessionsService;
        private readonly CurrentUserService _currentUserService;
        private List<SessionDisplay> _allSessions = new();

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;

        [ObservableProperty] private bool isUpcomingTabActive = true;
        [ObservableProperty] private bool isCompletedTabActive;
        [ObservableProperty] private bool isCancelledTabActive;

        [ObservableProperty] private bool isCancelPromptVisible;
        [ObservableProperty] private string cancelReasonText = string.Empty;
        private SessionDisplay? _pendingCancelTarget;

        public ObservableCollection<SessionDisplay> UpcomingSessions { get; } = new();
        public ObservableCollection<SessionDisplay> CompletedSessions { get; } = new();
        public ObservableCollection<SessionDisplay> CancelledSessions { get; } = new();

        public SessionsViewModel(SessionsService sessionsService, CurrentUserService currentUserService)
        {
            _sessionsService = sessionsService;
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
                _allSessions = await _sessionsService.GetSessionsForUserAsync(user.UserId);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load sessions: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            UpcomingSessions.Clear();
            foreach (var s in _allSessions.Where(s => s.Status == "Upcoming"))
                UpcomingSessions.Add(s);

            CompletedSessions.Clear();
            foreach (var s in _allSessions.Where(s => s.Status == "Completed"))
                CompletedSessions.Add(s);

            CancelledSessions.Clear();
            foreach (var s in _allSessions.Where(s => s.Status == "Cancelled"))
                CancelledSessions.Add(s);
        }

        [RelayCommand]
        private void ShowUpcomingTab()
        {
            IsUpcomingTabActive = true;
            IsCompletedTabActive = false;
            IsCancelledTabActive = false;
        }

        [RelayCommand]
        private void ShowCompletedTab()
        {
            IsUpcomingTabActive = false;
            IsCompletedTabActive = true;
            IsCancelledTabActive = false;
        }

        [RelayCommand]
        private void ShowCancelledTab()
        {
            IsUpcomingTabActive = false;
            IsCompletedTabActive = false;
            IsCancelledTabActive = true;
        }

        [RelayCommand]
        private void StartCancel(SessionDisplay session)
        {
            if (session is null) return;
            _pendingCancelTarget = session;
            CancelReasonText = string.Empty;
            IsCancelPromptVisible = true;
        }

        [RelayCommand]
        private async Task ConfirmCancelAsync()
        {
            if (_pendingCancelTarget is null || IsBusy) return;
            if (string.IsNullOrWhiteSpace(CancelReasonText))
            {
                StatusMessage = "Please provide a reason.";
                return;
            }
            IsBusy = true;
            try
            {
                bool success = await _sessionsService.CancelSessionAsync(_pendingCancelTarget.SessionId, CancelReasonText);
                if (success)
                {
                    UpcomingSessions.Remove(_pendingCancelTarget);
                    _pendingCancelTarget.Status = "Cancelled";
                    _pendingCancelTarget.CancellationReason = CancelReasonText;
                    CancelledSessions.Add(_pendingCancelTarget);
                    IsCancelPromptVisible = false;
                    _pendingCancelTarget = null;
                }
                else
                {
                    StatusMessage = "Could not cancel session.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelCancel()
        {
            IsCancelPromptVisible = false;
            _pendingCancelTarget = null;
            CancelReasonText = string.Empty;
        }

        [RelayCommand]
        private async Task CompleteAsync(SessionDisplay session)
        {
            if (session is null || IsBusy || !session.IsTutorView) return;
            IsBusy = true;
            try
            {
                bool success = await _sessionsService.CompleteSessionAsync(session.SessionId);
                if (success)
                {
                    UpcomingSessions.Remove(session);
                    session.Status = "Completed";
                    CompletedSessions.Add(session);
                }
                else
                {
                    StatusMessage = "Could not mark session complete.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}