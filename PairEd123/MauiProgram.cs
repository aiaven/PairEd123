using Microsoft.Extensions.Logging;
using PairEd123.Services;
using PairEd123.ViewModels;
using PairEd123.Views;

namespace PairEd123
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<AuthViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddSingleton<CurrentUserService>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<Views.ProfilePage>();
            builder.Services.AddTransient<UserDashboardViewModel>();
            builder.Services.AddTransient<Views.UserDashboardPage>();
            builder.Services.AddTransient<AdminDashboardViewModel>();
            builder.Services.AddTransient<Views.AdminDashboardPage>();
            builder.Services.AddTransient<SearchService>();
            builder.Services.AddTransient<SearchViewModel>();
            builder.Services.AddTransient<Views.SearchPage>();
            builder.Services.AddTransient<SkillsService>();
            builder.Services.AddTransient<AdminSkillsViewModel>();
            builder.Services.AddTransient<Views.AdminSkillsPage>();
            builder.Services.AddTransient<UserManagementService>();
            builder.Services.AddTransient<ManageUsersViewModel>();
            builder.Services.AddTransient<Views.ManageUsersPage>();
            builder.Services.AddSingleton<RequestsService>();
            builder.Services.AddTransient<TutorDetailViewModel>();
            builder.Services.AddTransient<Views.TutorDetailPage>();
            builder.Services.AddTransient<RequestsViewModel>();
            builder.Services.AddTransient<Views.RequestsPage>();
            builder.Services.AddTransient<SessionsService>();
            builder.Services.AddTransient<SessionsViewModel>();
            builder.Services.AddTransient<Views.SessionsPage>();
            builder.Services.AddTransient<FeedbackService>();
            builder.Services.AddTransient<FeedbackViewModel>();
            builder.Services.AddTransient<Views.FeedbackPage>();
            builder.Services.AddTransient<AdminFeedbackViewModel>();
            builder.Services.AddTransient<Views.AdminFeedbackPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}