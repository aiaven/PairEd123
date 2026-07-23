using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class UserDashboardPage : ContentPage
    {
        private readonly UserDashboardViewModel _vm;

        public UserDashboardPage(UserDashboardViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCurrentUser();
        }
    }
}