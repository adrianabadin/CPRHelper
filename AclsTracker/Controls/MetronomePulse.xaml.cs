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
    /// Quick scale pulse: expand to 1.15x then back to 1.0x.
    /// Total duration ~200ms for snappy visual feedback at up to 120 BPM (500ms interval).
    /// Uses Easing.CubicOut for natural deceleration feel.
    /// </summary>
    private async Task AnimatePulse()
    {
        // Cancel any running animation
        PulseCircle.CancelAnimations();

        // Expand
        await PulseCircle.ScaleTo(1.15, 80, Easing.CubicOut);
        // Contract back
        await PulseCircle.ScaleTo(1.0, 120, Easing.CubicIn);
    }
}