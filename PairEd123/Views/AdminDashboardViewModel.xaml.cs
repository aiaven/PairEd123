using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class AdminDashboardPage : ContentPage
    {
        private readonly AdminDashboardViewModel _vm;

        public AdminDashboardPage(AdminDashboardViewModel vm)
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