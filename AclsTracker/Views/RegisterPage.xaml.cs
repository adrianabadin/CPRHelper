using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
