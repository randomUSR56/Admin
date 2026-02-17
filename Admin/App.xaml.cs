using Admin.Services;

namespace Admin
{
    public partial class App : Application
    {
        private readonly AuthTokenStore _tokenStore;

        public App(AuthTokenStore tokenStore)
        {
            InitializeComponent();
            _tokenStore = tokenStore;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();
            var window = new Window(shell);

            shell.Loaded += async (s, e) =>
            {
                if (await _tokenStore.HasTokenAsync())
                    await Shell.Current.GoToAsync("//Main/Dashboard");
                else
                    await Shell.Current.GoToAsync("//Login/LoginPage");
            };

            return window;
        }
    }
}