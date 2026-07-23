using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Services;
namespace PairEd123.ViewModels
{
    public partial class UserDashboardViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        [ObservableProperty] private string greeting = string.Empty;
        [ObservableProperty] private string roleLabel = string.Empty;
        [ObservableProperty] private string? profilePicturePath;
        public UserDashboardViewModel(CurrentUserService currentUserService)
            => _currentUserService = currentUserService;
        public void LoadCurrentUser()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null)
            {
                Greeting = "Welcome";
                return;
            }
            Greeting = $"Welcome, {user.DisplayName}";
            RoleLabel = user.IsTutor ? "Tutor" : "Student";
            ProfilePicturePath = user.ProfilePicture;
        }
        [RelayCommand]
        private async Task GoToProfileAsync() => await Shell.Current.GoToAsync(nameof(Views.ProfilePage));
        [RelayCommand]
        private async Task GoToSearchAsync() => await Shell.Current.GoToAsync(nameof(Views.SearchPage));
        [RelayCommand]
        private async Task GoToRequestsAsync() => await Shell.Current.GoToAsync(nameof(Views.RequestsPage));
        [RelayCommand]
        private async Task GoToSessionsAsync() => await Shell.Current.GoToAsync(nameof(Views.SessionsPage));
        [RelayCommand]
        private async Task GoToFeedbackAsync() => await Shell.Current.GoToAsync(nameof(Views.FeedbackPage));
        [RelayCommand]
        private async Task LogoutAsync()
        {
            _currentUserService.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}