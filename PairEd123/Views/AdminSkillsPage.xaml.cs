using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class AdminSkillsPage : ContentPage
    {
        private readonly AdminSkillsViewModel _viewModel;

        public AdminSkillsPage(AdminSkillsViewModel viewModel)
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