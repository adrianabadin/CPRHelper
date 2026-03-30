using System.Windows.Input;

namespace AclsTracker.Controls;

public partial class TimerCard : ContentView
{
    /// <summary>
    /// When true, shows the pause/resume button for the compressions timer.
    /// </summary>
    public static readonly BindableProperty ShowPauseButtonProperty =
        BindableProperty.Create(nameof(ShowPauseButton), typeof(bool), typeof(TimerCard), false);

    public bool ShowPauseButton
    {
        get => (bool)GetValue(ShowPauseButtonProperty);
        set => SetValue(ShowPauseButtonProperty, value);
    }

    /// <summary>
    /// Command invoked when the pause/resume button is tapped.
    /// Should be bound to TimerViewModel.ToggleCompressionsPause.
    /// </summary>
    public static readonly BindableProperty PauseCommandProperty =
        BindableProperty.Create(nameof(PauseCommand), typeof(ICommand), typeof(TimerCard), null);

    public ICommand? PauseCommand
    {
        get => (ICommand?)GetValue(PauseCommandProperty);
        set => SetValue(PauseCommandProperty, value);
    }

    public TimerCard()
    {
        InitializeComponent();
    }
}
