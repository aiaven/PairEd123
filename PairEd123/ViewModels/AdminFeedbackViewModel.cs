using CommunityToolkit.Mvvm.ComponentModel;
using PairEd123.Models;
using PairEd123.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace PairEd123.ViewModels
{
    public partial class AdminFeedbackViewModel : ObservableObject
    {
        private readonly FeedbackService _feedbackService;
        private List<FeedbackDisplay> _allFeedback = new();

        [ObservableProperty] private string queryText = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;

        public ObservableCollection<FeedbackDisplay> Results { get; } = new();

        public AdminFeedbackViewModel(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            try
            {
                _allFeedback = await _feedbackService.GetAllFeedbackAsync();
                ApplyFilter();
                StatusMessage = _allFeedback.Count == 0 ? "No feedback submitted yet." : string.Empty;
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

        partial void OnQueryTextChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            Results.Clear();
            var filtered = string.IsNullOrWhiteSpace(QueryText)
                ? _allFeedback
                : _allFeedback.Where(f => f.OtherPartyName.Contains(QueryText, StringComparison.OrdinalIgnoreCase));
            foreach (var f in filtered)
                Results.Add(f);
        }
    }
}