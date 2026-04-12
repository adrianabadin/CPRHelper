namespace AclsTracker.Controls;

/// <summary>
/// Visual metronome pulse circle.
/// Flashes bright on each beat synchronized with the audio click.
/// </summary>
public partial class MetronomePulse : ContentView
{
    private static readonly Color RestColor = Color.FromArgb("#D32F2F");
    private static readonly Color FlashColor = Color.FromArgb("#FF6659");

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

    private static void OnIsPulsingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MetronomePulse control)
        {
            control.AnimatePulse();
        }
    }

    /// <summary>
    /// Color flash animation: instantly flash bright, then animate back to rest color.
    /// Uses Animation API which is the most reliable animation method in MAUI.
    /// </summary>
    private void AnimatePulse()
    {
        // Cancel any running animation
        this.AbortAnimation("HeartbeatPulse");

        // Instant flash to bright color
        PulseCircle.Color = FlashColor;

        // Animate from flash back to rest over 300ms
        var animation = new Animation(v =>
        {
            float r = (float)(FlashColor.Red + (RestColor.Red - FlashColor.Red) * v);
            float g = (float)(FlashColor.Green + (RestColor.Green - FlashColor.Green) * v);
            float b = (float)(FlashColor.Blue + (RestColor.Blue - FlashColor.Blue) * v);
            PulseCircle.Color = new Color(r, g, b);
        }, 0, 1, Easing.CubicIn);

        animation.Commit(this, "HeartbeatPulse", length: 300);
    }
}