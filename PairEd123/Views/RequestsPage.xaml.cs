using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class RequestsPage : ContentPage
    {
        private readonly RequestsViewModel _vm;
        public RequestsPage(RequestsViewModel vm)
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