using PairEd123.ViewModels;

namespace PairEd123.Views
{
    public partial class TutorDetailPage : ContentPage
    {
        public TutorDetailPage(TutorDetailViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}