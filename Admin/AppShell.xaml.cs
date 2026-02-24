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
        }
    }
}
