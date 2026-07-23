using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using PairEd123.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
namespace PairEd123.ViewModels
{
    [QueryProperty(nameof(TutorId), "tutorId")]
    [QueryProperty(nameof(TutorName), "tutorName")]
    public partial class TutorDetailViewModel : ObservableObject
    {
        private readonly SkillsService _skillsService;
        private readonly RequestsService _requestsService;
        private readonly CurrentUserService _currentUserService;
        [ObservableProperty] private int tutorId;
        [ObservableProperty] private string tutorName = string.Empty;
        [ObservableProperty] private string? selectedSkill;
        [ObservableProperty] private DateTime preferredDate = DateTime.Today;
        [ObservableProperty] private TimeSpan preferredTime = new(9, 0, 0);
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;
        public ObservableCollection<string> SkillNames { get; } = new();
        public TutorDetailViewModel(SkillsService skillsService, RequestsService requestsService, CurrentUserService currentUserService)
        {
            _skillsService = skillsService;
            _requestsService = requestsService;
            _currentUserService = currentUserService;
        }
        async partial void OnTutorIdChanged(int value)
        {
            await LoadSkillsAsync();
        }
        private async Task LoadSkillsAsync()
        {
            if (TutorId == 0) return;
            IsBusy = true;
            try
            {
                var skills = await _skillsService.GetSkillsForTutorAsync(TutorId);
                SkillNames.Clear();
                foreach (var s in skills.Select(s => s.SkillName).Distinct())
                    SkillNames.Add(s);
                SelectedSkill = SkillNames.FirstOrDefault();
                if (SkillNames.Count == 0)
                    StatusMessage = "This tutor hasn't listed any skills yet.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load skills: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task SendRequestAsync()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null)
            {
                StatusMessage = "No user logged in.";
                return;
            }
            if (user.UserId == TutorId)
            {
                StatusMessage = "You can't send a request to yourself.";
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedSkill))
            {
                StatusMessage = "Please select a skill.";
                return;
            }
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Sending request...";
            try
            {
                var combined = PreferredDate.Date + PreferredTime;
                var (success, message) = await _requestsService.CreateRequestAsync(
                    user.UserId, TutorId, SelectedSkill, combined);
                StatusMessage = message;
                if (success)
                    await Shell.Current.GoToAsync("..");
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
    }
}