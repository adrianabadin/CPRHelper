using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class ProfilePage : ContentPage
{
    private readonly AuthViewModel _viewModel;

    public ProfilePage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProfileCommand.ExecuteAsync(null);
    }
}
