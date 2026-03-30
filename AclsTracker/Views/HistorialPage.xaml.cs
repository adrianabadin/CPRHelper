using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class HistorialPage : ContentPage
{
    private readonly HistorialViewModel _viewModel;

    public HistorialPage(HistorialViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh saved sessions list when tab is selected (in case new session was saved)
        if (_viewModel.IsSavedView)
        {
            await _viewModel.LoadSavedSessions();
        }
    }
}
