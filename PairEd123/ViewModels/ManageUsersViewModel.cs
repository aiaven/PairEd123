using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Models;
using PairEd123.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PairEd123.ViewModels
{
    public partial class ManageUsersViewModel : ObservableObject
    {
        private readonly UserManagementService _userService;
        public ManageUsersViewModel(UserManagementService userService) => _userService = userService;
        public ObservableCollection<User> Users { get; } = new();
        public List<string> RoleOptions { get; } = new() { "Student", "Tutor", "Admin" };
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                var users = await _userService.GetAllUsersAsync();
                Users.Clear();
                foreach (var u in users) Users.Add(u);
            }
            finally { IsBusy = false; }
        }
        [RelayCommand]
        private async Task ChangeRoleAsync(User user)
        {
            if (user is null || IsBusy) return;
            IsBusy = true;
            try
            {
                bool success = await _userService.UpdateRoleAsync(user.UserId, user.Role);
                StatusMessage = success ? $"{user.DisplayName} is now {user.Role}." : "Update failed.";
            }
            finally { IsBusy = false; }
        }
    }
}