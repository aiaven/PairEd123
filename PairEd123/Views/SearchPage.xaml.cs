using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly SearchViewModel _viewModel;

        public SearchPage(SearchViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SearchViewModel vm)
            {
                await vm.LoadAllTutorsAsync();
            }
        }
    }
}