using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
