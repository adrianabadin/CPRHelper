using AclsTracker.Views;

namespace AclsTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for modal authentication pages
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
    }
}