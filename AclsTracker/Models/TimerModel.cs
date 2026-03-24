using CommunityToolkit.Mvvm.ComponentModel;

namespace AclsTracker.Models;

public partial class TimerModel : ObservableObject
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public TimerType Type { get; init; }

    [ObservableProperty]
    private TimeSpan _elapsed = TimeSpan.Zero;

    [ObservableProperty]
    private bool _isRunning;

    /// <summary>
    /// Target duration for progress circle display (e.g., 2 min for CPR cycle).
    /// Null means no target (elapsed-only timer like TotalElapsed).
    /// </summary>
    public TimeSpan? TargetDuration { get; init; }

    /// <summary>
    /// Progress percentage 0.0-1.0 for circular progress indicator (per D-05).
    /// Returns 0 if no target duration is set.
    /// </summary>
    public double ProgressPercentage =>
        TargetDuration.HasValue && TargetDuration.Value.TotalMilliseconds > 0
            ? Math.Min(Elapsed.TotalMilliseconds / TargetDuration.Value.TotalMilliseconds, 1.0)
            : 0.0;

    partial void OnElapsedChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
    }
}