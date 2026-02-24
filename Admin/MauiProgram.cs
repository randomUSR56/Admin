using Microsoft.Extensions.Logging;
using Admin.Services;
using Admin.ViewModels;
using Admin.Views;

namespace Admin
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

            // Services
            builder.Services.AddSingleton<AuthTokenStore>();

            builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                // TODO: Update this to your Laravel backend URL
                client.BaseAddress = new Uri("http://onlyfix.local");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<UsersViewModel>();
            builder.Services.AddTransient<UserDetailViewModel>();
            builder.Services.AddTransient<ProblemsViewModel>();
            builder.Services.AddTransient<ProblemDetailViewModel>();
            builder.Services.AddTransient<CarsViewModel>();
            builder.Services.AddTransient<CarDetailViewModel>();
            builder.Services.AddTransient<TicketsViewModel>();
            builder.Services.AddTransient<TicketDetailViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<UsersPage>();
            builder.Services.AddTransient<UserDetailPage>();
            builder.Services.AddTransient<ProblemsPage>();
            builder.Services.AddTransient<ProblemDetailPage>();
            builder.Services.AddTransient<CarsPage>();
            builder.Services.AddTransient<CarDetailPage>();
            builder.Services.AddTransient<TicketsPage>();
            builder.Services.AddTransient<TicketDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
