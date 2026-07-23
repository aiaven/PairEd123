using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class AdminFeedbackPage : ContentPage
    {
        private readonly AdminFeedbackViewModel _vm;
        public AdminFeedbackPage(AdminFeedbackViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadAsync();
        }
    }
}