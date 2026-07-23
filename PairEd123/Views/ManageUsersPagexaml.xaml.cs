using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class ManageUsersPage : ContentPage
    {
        private readonly ManageUsersViewModel _viewModel;

        public ManageUsersPage(ManageUsersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}