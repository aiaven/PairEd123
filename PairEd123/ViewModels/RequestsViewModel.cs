using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;

namespace PairEd123.ViewModels
{
    public partial class RequestsViewModel : ObservableObject
    {
        private readonly RequestsService _requestsService;
        private readonly SessionsService _sessionsService;
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty] private bool isTutor;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;

        [ObservableProperty] private bool isSentTabActive = true;
        [ObservableProperty] private bool isIncomingTabActive;

        [ObservableProperty] private bool isDeclinePromptVisible;
        [ObservableProperty] private string declineReasonText = string.Empty;
        private RequestDisplay? _pendingDeclineTarget;

        public ObservableCollection<RequestDisplay> SentRequests { get; } = new();
        public ObservableCollection<RequestDisplay> IncomingRequests { get; } = new();

        public RequestsViewModel(RequestsService requestsService, SessionsService sessionsService, CurrentUserService currentUserService)
        {
            _requestsService = requestsService;
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
            IsTutor = user.IsTutor;
            IsBusy = true;
            try
            {
                var sent = await _requestsService.GetSentRequestsAsync(user.UserId);
                SentRequests.Clear();
                foreach (var r in sent) SentRequests.Add(r);

                if (IsTutor)
                {
                    var incoming = await _requestsService.GetIncomingRequestsAsync(user.UserId);
                    IncomingRequests.Clear();
                    foreach (var r in incoming) IncomingRequests.Add(r);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load requests: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ShowSentTab()
        {
            IsSentTabActive = true;
            IsIncomingTabActive = false;
        }

        [RelayCommand]
        private void ShowIncomingTab()
        {
            IsSentTabActive = false;
            IsIncomingTabActive = true;
        }

        [RelayCommand]
        private async Task AcceptAsync(RequestDisplay request)
        {
            if (request is null || IsBusy) return;
            IsBusy = true;
            try
            {
                bool success = await _requestsService.AcceptRequestAsync(request.RequestId);
                if (success)
                {
                    IncomingRequests.Remove(request);

                    var (sessionSuccess, sessionMessage) = await _sessionsService.CreateSessionAsync(
                        request.RequestId, request.TutorId, request.TuteeId, request.Subject, request.PreferredDate);
                    if (!sessionSuccess)
                        StatusMessage = $"Request accepted, but session creation failed: {sessionMessage}";
                }
                else
                {
                    StatusMessage = "Could not accept request.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void StartDecline(RequestDisplay request)
        {
            if (request is null) return;
            _pendingDeclineTarget = request;
            DeclineReasonText = string.Empty;
            IsDeclinePromptVisible = true;
        }

        [RelayCommand]
        private async Task ConfirmDeclineAsync()
        {
            if (_pendingDeclineTarget is null || IsBusy) return;
            if (string.IsNullOrWhiteSpace(DeclineReasonText))
            {
                StatusMessage = "Please provide a reason.";
                return;
            }
            IsBusy = true;
            try
            {
                bool success = await _requestsService.DeclineRequestAsync(_pendingDeclineTarget.RequestId, DeclineReasonText);
                if (success)
                {
                    IncomingRequests.Remove(_pendingDeclineTarget);
                    IsDeclinePromptVisible = false;
                    _pendingDeclineTarget = null;
                }
                else
                {
                    StatusMessage = "Could not decline request.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelDecline()
        {
            IsDeclinePromptVisible = false;
            _pendingDeclineTarget = null;
            DeclineReasonText = string.Empty;
        }
    }
}