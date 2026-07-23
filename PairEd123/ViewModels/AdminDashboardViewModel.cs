using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Services;
namespace PairEd123.ViewModels
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        [ObservableProperty] private string greeting = string.Empty;
        public AdminDashboardViewModel(CurrentUserService currentUserService)
            => _currentUserService = currentUserService;
        public void LoadCurrentUser()
        {
            var user = _currentUserService.CurrentUser;
            Greeting = user is null ? "Welcome, Admin" : $"Welcome, {user.DisplayName} (Admin)";
        }
        [RelayCommand]
        private async Task GoToManageUsersAsync() => await Shell.Current.GoToAsync(nameof(Views.ManageUsersPage));
        [RelayCommand]
        private async Task GoToManageSkillsAsync() => await Shell.Current.GoToAsync(nameof(Views.AdminSkillsPage));
        [RelayCommand]
        private async Task GoToReportsAsync() => await Shell.Current.GoToAsync(nameof(Views.AdminFeedbackPage));
        [RelayCommand]
        private async Task LogoutAsync()
        {
            _currentUserService.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}