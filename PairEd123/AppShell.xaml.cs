using PairEd123.Views;

namespace PairEd123
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
            Routing.RegisterRoute(nameof(AdminSkillsPage), typeof(AdminSkillsPage));
            Routing.RegisterRoute(nameof(ManageUsersPage), typeof(ManageUsersPage));
            Routing.RegisterRoute(nameof(Views.TutorDetailPage), typeof(Views.TutorDetailPage));
            Routing.RegisterRoute(nameof(RequestsPage), typeof(RequestsPage));
            Routing.RegisterRoute(nameof(SessionsPage), typeof(SessionsPage));
            Routing.RegisterRoute(nameof(FeedbackPage), typeof(FeedbackPage));
            Routing.RegisterRoute(nameof(AdminFeedbackPage), typeof(AdminFeedbackPage));
            // Future pages go here too, same pattern:
            // Routing.RegisterRoute(nameof(RequestsPage), typeof(RequestsPage));
        }
    }
}