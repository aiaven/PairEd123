using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;
namespace PairEd123.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly CurrentUserService _currentUserService;
        public AuthViewModel(AuthService authService, CurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }
        [ObservableProperty] private string emailAddress = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string displayName = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(EmailAddress) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please fill in all fields.";
                return;
            }
            if (Password != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match.";
                return;
            }
            IsBusy = true;
            StatusMessage = "Connecting...";
            try
            {
                // isTutor defaults to false at signup; can be enabled later from Profile.
                var (success, message) = await _authService.RegisterAsync(
                    EmailAddress, Password, DisplayName, isTutor: false);
                StatusMessage = message;
                if (success)
                    await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Connecting...";
            try
            {
                var user = await _authService.LoginAsync(EmailAddress, Password);
                if (user is null)
                {
                    StatusMessage = "Invalid email or password.";
                    return;
                }
                _currentUserService.SetUser(user);
                StatusMessage = string.Empty;
                if (user.Role == "Admin")
                    await Shell.Current.GoToAsync("//AdminDashboardPage");
                else
                    await Shell.Current.GoToAsync("//UserDashboardPage");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task GoToRegisterAsync() => await Shell.Current.GoToAsync(nameof(Views.RegisterPage));
        [RelayCommand]
        private async Task GoToLoginAsync() => await Shell.Current.GoToAsync("//LoginPage");
    }
}