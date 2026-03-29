using AclsTracker.Controls;
using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>
    /// Show a notification banner with a message that auto-dismisses.
    /// </summary>
    public void ShowNotification(string message, int autoDismissSeconds = 5)
    {
        BannerControl.Show(message, autoDismissSeconds);
    }
}