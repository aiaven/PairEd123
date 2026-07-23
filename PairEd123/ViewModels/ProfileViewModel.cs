using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PairEd123.Services;

namespace PairEd123.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly CurrentUserService _currentUserService;
        private readonly SkillsService _skillsService;

        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string displayName = string.Empty;
        [ObservableProperty] private string? profilePicturePath;
        [ObservableProperty] private bool isTutor;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? availability;
        [ObservableProperty] private string? bio;
        [ObservableProperty] private bool isSkillsBusy;
        [ObservableProperty] private bool hasCustomPhoto;

        public ObservableCollection<SkillPickItem> Skills { get; } = new();

        partial void OnProfilePicturePathChanged(string? value)
        {
            HasCustomPhoto = !string.IsNullOrEmpty(value);
        }
        public ProfileViewModel(AuthService authService, CurrentUserService currentUserService, SkillsService skillsService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            _skillsService = skillsService;
        }

        public void LoadCurrentUser()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null)
            {
                StatusMessage = "No user logged in.";
                return;
            }
            Email = user.Email;
            DisplayName = user.DisplayName;
            ProfilePicturePath = user.ProfilePicture;
            IsTutor = user.IsTutor;
            Availability = user.Availability;
            Bio = user.Bio;
        }

        public async Task LoadSkillsAsync()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null || !IsTutor) return;

            IsSkillsBusy = true;
            try
            {
                var catalog = await _skillsService.GetCatalogAsync();
                var mine = await _skillsService.GetSkillsForTutorAsync(user.UserId);

                Skills.Clear();
                foreach (var skill in catalog)
                {
                    var match = mine.FirstOrDefault(m => m.SkillName == skill.SkillName);
                    Skills.Add(new SkillPickItem
                    {
                        SkillId = skill.SkillId,
                        SkillName = skill.SkillName,
                        IsSelected = match is not null,
                        UserSkillRowId = match?.SkillId
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load skills: {ex.Message}";
            }
            finally
            {
                IsSkillsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ToggleSkillAsync(SkillPickItem item)
        {
            var user = _currentUserService.CurrentUser;
            if (user is null || item is null) return;

            IsSkillsBusy = true;
            try
            {
                if (item.IsSelected)
                {
                    if (item.UserSkillRowId is int rowId)
                    {
                        var removed = await _skillsService.RemoveSkillFromTutorAsync(rowId);
                        if (removed)
                        {
                            item.IsSelected = false;
                            item.UserSkillRowId = null;
                        }
                        else
                        {
                            StatusMessage = "Could not remove skill.";
                        }
                    }
                }
                else
                {
                    var (success, message) = await _skillsService.AddSkillToTutorAsync(user.UserId, item.SkillName);
                    if (success)
                    {
                        var mine = await _skillsService.GetSkillsForTutorAsync(user.UserId);
                        var match = mine.FirstOrDefault(m => m.SkillName == item.SkillName);
                        item.UserSkillRowId = match?.SkillId;
                        item.IsSelected = true;
                    }
                    else
                    {
                        StatusMessage = message;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                IsSkillsBusy = false;
            }
        }

        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                string? choice = await Application.Current!.Windows[0].Page!.DisplayActionSheet(
                    "Choose photo from", "Cancel", null, "Photo Gallery", "Files");

                if (choice is null || choice == "Cancel") return;

                FileResult? photo = choice == "Photo Gallery"
                    ? await MediaPicker.Default.PickPhotoAsync()
                    : await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Select an image",
                        FileTypes = FilePickerFileType.Images
                    });

                if (photo is null) return;

                string localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                await using (var sourceStream = await photo.OpenReadAsync())
                await using (var localStream = File.OpenWrite(localPath))
                {
                    await sourceStream.CopyToAsync(localStream);
                }
                ProfilePicturePath = localPath;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Photo pick failed: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var user = _currentUserService.CurrentUser;
            if (user is null)
            {
                StatusMessage = "No user logged in.";
                return;
            }
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Saving...";
            try
            {
                var (success, message) = await _authService.UpdateProfileAsync(
                    user.UserId, DisplayName, ProfilePicturePath, IsTutor, Availability, Bio);
                StatusMessage = message;
                if (success)
                {
                    user.DisplayName = DisplayName;
                    user.ProfilePicture = ProfilePicturePath;
                    user.IsTutor = IsTutor;
                    user.Availability = Availability;
                    user.Bio = Bio;
                    _currentUserService.SetUser(user);

                    if (IsTutor)
                        await LoadSkillsAsync();
                }
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