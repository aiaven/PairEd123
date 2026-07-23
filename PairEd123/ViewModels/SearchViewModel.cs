using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PairEd123.Models;
using PairEd123.Services;
using CommunityToolkit.Mvvm.Input;
namespace PairEd123.ViewModels
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly SearchService _searchService;
        private readonly CurrentUserService _currentUserService;
        private List<TutorSearchResult> _allTutors = new();
        public SearchViewModel(SearchService searchService, CurrentUserService currentUserService)
        {
            _searchService = searchService;
            _currentUserService = currentUserService;
        }
        [ObservableProperty] private string queryText = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;
        public ObservableCollection<TutorSearchResult> Results { get; } = new();
        public async Task LoadAllTutorsAsync()
        {
            IsBusy = true;
            StatusMessage = "Loading tutors...";
            try
            {
                var tutors = await _searchService.GetAllTutorsAsync();
                var currentUserId = _currentUserService.CurrentUser?.UserId;
                _allTutors = tutors.Where(t => t.UserId != currentUserId).ToList();
                ApplyFilter();
                StatusMessage = _allTutors.Count == 0 ? "No tutors available yet." : string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load tutors: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        partial void OnQueryTextChanged(string value) => ApplyFilter();
        private void ApplyFilter()
        {
            Results.Clear();
            var filtered = string.IsNullOrWhiteSpace(QueryText)
                ? _allTutors
                : _allTutors.Where(t =>
                    t.DisplayName.Contains(QueryText, StringComparison.OrdinalIgnoreCase) ||
                    t.Skills.Contains(QueryText, StringComparison.OrdinalIgnoreCase));
            foreach (var t in filtered)
                Results.Add(t);
        }

        [RelayCommand]
        private async Task GoToTutorDetailAsync(TutorSearchResult tutor)
        {
            if (tutor is null) return;
            await Shell.Current.GoToAsync(
                $"{nameof(Views.TutorDetailPage)}?tutorId={tutor.UserId}&tutorName={Uri.EscapeDataString(tutor.DisplayName)}");
        }
    }
}