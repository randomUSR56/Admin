using Admin.Views;

namespace Admin
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("UserDetail", typeof(UserDetailPage));
            Routing.RegisterRoute("ProblemDetail", typeof(ProblemDetailPage));
            Routing.RegisterRoute("CarDetail", typeof(CarDetailPage));
            Routing.RegisterRoute("TicketDetail", typeof(TicketDetailPage));

            // Listen for navigation to toggle flyout visibility
            Navigated += OnShellNavigated;
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            // Hide the flyout sidebar on the login page, show it everywhere else
            var location = Current?.CurrentState?.Location?.OriginalString ?? string.Empty;
            if (location.Contains("LoginPage"))
            {
                FlyoutBehavior = FlyoutBehavior.Disabled;
            }
            else
            {
                FlyoutBehavior = FlyoutBehavior.Locked;
            }
        }
    }
}
