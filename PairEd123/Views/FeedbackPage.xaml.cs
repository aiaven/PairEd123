using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class FeedbackPage : ContentPage
    {
        private readonly FeedbackViewModel _vm;
        public FeedbackPage(FeedbackViewModel vm)
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