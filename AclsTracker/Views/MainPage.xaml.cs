using System.Windows.Input;
using AclsTracker.Controls;
using AclsTracker.ViewModels;

namespace AclsTracker.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    /// <summary>
    /// Exposes TimerViewModel's toggle pause command for the TimerCard pause button.
    /// </summary>
    public ICommand TogglePauseCommand => _viewModel.Timer.ToggleCompressionsPauseToggleCommand;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.DefibrillationTriggered += OnDefibrillationTriggered;
        _viewModel.PulseCheckRequired += OnPulseCheckRequired;
        _viewModel.RhythmPopupRequired += OnRhythmPopupRequired;
    }

    private async void OnRhythmPopupRequired(string title, string message)
    {
        var page = new RhythmPopupPage(title, message);
        await Navigation.PushModalAsync(page);
    }

    private async void OnPulseCheckRequired(List<string> suggestions)
    {
        var page = new PulseCheckPage(suggestions, () => _viewModel.PauseForPulseCheck());
        await Navigation.PushModalAsync(page);
    }

    private async void OnDefibrillationTriggered()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DefibrilarButton.FadeTo(0.3, 100);
            await DefibrilarButton.FadeTo(1.0, 200);
            await DefibrilarButton.ScaleTo(1.1, 100, Easing.CubicOut);
            await DefibrilarButton.ScaleTo(1.0, 150, Easing.CubicIn);
        }
        catch
        {
            // Swallow animation errors — emergency app must not crash on animation failure
        }
    }

    /// <summary>
    /// Show a notification banner with a message that auto-dismisses.
    /// </summary>
    public void ShowNotification(string message, int autoDismissSeconds = 5)
    {
        BannerControl.Show(message, autoDismissSeconds);
    }
}
