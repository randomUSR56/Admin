using Admin.Views;

namespace Admin
{
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Width threshold (in logical pixels) above which the sidebar locks open.
        /// Below this the sidebar collapses and a hamburger button is shown instead.
        /// </summary>
        private const double WideBreakpoint = 900;

        private bool _isOnLoginPage = true;

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("UserDetail", typeof(UserDetailPage));
            Routing.RegisterRoute("ProblemDetail", typeof(ProblemDetailPage));
            Routing.RegisterRoute("CarDetail", typeof(CarDetailPage));
            Routing.RegisterRoute("TicketDetail", typeof(TicketDetailPage));

            Navigated += OnShellNavigated;
        }

        // Called once the Shell is fully attached to its native window handler.
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Window is not null)
            {
                Window.SizeChanged += OnWindowSizeChanged;
                // Apply initial behavior based on startup window size.
                UpdateFlyoutBehavior();
            }
        }

        private void OnWindowSizeChanged(object? sender, EventArgs e)
            => UpdateFlyoutBehavior();

        private void UpdateFlyoutBehavior()
        {
            if (_isOnLoginPage)
            {
                FlyoutBehavior = FlyoutBehavior.Disabled;
                return;
            }

            var width = Window?.Width ?? 0;

            // Wide window  → sidebar is always visible (locked).
            // Narrow window → sidebar collapses; Shell renders a hamburger button
            //                 automatically in the nav bar when FlyoutBehavior.Flyout.
            FlyoutBehavior = width >= WideBreakpoint
                ? FlyoutBehavior.Locked
                : FlyoutBehavior.Flyout;
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            var location = Current?.CurrentState?.Location?.OriginalString ?? string.Empty;
            _isOnLoginPage = location.Contains("LoginPage");
            UpdateFlyoutBehavior();
        }
    }
}
