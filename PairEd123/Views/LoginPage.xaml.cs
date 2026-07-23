using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(AuthViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}