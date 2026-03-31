using AclsTracker.Views;
using AclsTracker.ViewModels;

namespace AclsTracker;

public partial class AppShell : Shell
{
    public AppShell(AuthViewModel authViewModel)
    {
        InitializeComponent();
        
        authAvatar.BindingContext = authViewModel;

        // Register routes for modal authentication pages
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
    }
}