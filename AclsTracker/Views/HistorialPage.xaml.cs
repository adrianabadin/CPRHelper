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

    private async void OnBuscarClicked(object? sender, EventArgs e)
    {
        _viewModel.FromDate = FromDatePicker.Date;
        _viewModel.ToDate = ToDatePicker.Date;
        await _viewModel.LoadSavedSessions();
    }

    private async void OnLimpiarClicked(object? sender, EventArgs e)
    {
        FromDatePicker.Date = DateTime.Today;
        ToDatePicker.Date = DateTime.Today;
        _viewModel.SearchText = string.Empty;
        _viewModel.FromDate = null;
        _viewModel.ToDate = null;
        await _viewModel.LoadSavedSessions();
    }
}
