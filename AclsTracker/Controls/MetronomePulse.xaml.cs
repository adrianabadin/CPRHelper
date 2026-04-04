namespace AclsTracker.Controls;

/// <summary>
/// Visual metronome pulse circle (per D-03).
/// Animates scale 1.0 → 1.15 → 1.0 on each beat for visual feedback.
/// Uses CommunityToolkit.Maui animations targeting 60fps (AUDI-02).
/// </summary>
public partial class MetronomePulse : ContentView
{
    public static readonly BindableProperty IsPulsingProperty =
        BindableProperty.Create(
            nameof(IsPulsing),
            typeof(bool),
            typeof(MetronomePulse),
            false,
            propertyChanged: OnIsPulsingChanged);

    public bool IsPulsing
    {
        get => (bool)GetValue(IsPulsingProperty);
        set => SetValue(IsPulsingProperty, value);
    }

    public MetronomePulse()
    {
        InitializeComponent();
    }

    private static async void OnIsPulsingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MetronomePulse control)
        {
            await control.AnimatePulse();
        }
    }

    /// <summary>
    /// Heartbeat double-pump animation: lub (systole) → partial relax → dub (diastole) → full relax.
    /// Total duration ~330ms, fits within 500ms interval at 120 BPM.
    /// Scale and opacity run in parallel for maximum visual impact.
    /// </summary>
    private async Task AnimatePulse()
    {
        // Cancel any in-progress animation before starting a new beat
        PulseCircle.CancelAnimations();

        // First pump — systole "lub": scale to 1.35, opacity to 1.0
        await Task.WhenAll(
            PulseCircle.ScaleTo(1.35, 80, Easing.CubicOut),
            PulseCircle.FadeTo(1.0, 80)
        );

        // Quick partial relax between pumps
        await PulseCircle.ScaleTo(1.1, 60, Easing.CubicIn);

        // Second pump — diastole "dub": scale to 1.25, keep full opacity
        await PulseCircle.ScaleTo(1.25, 70, Easing.CubicOut);

        // Full relax back to resting state: scale to 1.0, fade back to 0.8
        await Task.WhenAll(
            PulseCircle.ScaleTo(1.0, 120, Easing.CubicIn),
            PulseCircle.FadeTo(0.8, 120)
        );
    }
}