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

    /// <summary>
    /// Returns true when timer has exceeded its target duration threshold.
    /// - PulseCheck: 10 seconds (during pulse check)
    /// - CprCycle: 2 minutes (protocol limit for CPR cycle)
    /// - Medication (Adrenalina): 4 minutes (AHA ACLS 2020 recommendation)
    /// - Amiodarona: 4 minutes (AHA ACLS 2020 recommendation)
    /// - TotalElapsed, Compressions: never red (no threshold)
    /// Used to trigger red background in TimerCard UI.
    /// </summary>
    public bool IsOverThreshold =>
        Type == TimerType.PulseCheck && Elapsed.TotalSeconds > 10 ||
        Type == TimerType.CprCycle && TargetDuration.HasValue && Elapsed.TotalSeconds > TargetDuration.Value.TotalSeconds ||
        Type == TimerType.Medication && TargetDuration.HasValue && Elapsed.TotalSeconds > TargetDuration.Value.TotalSeconds ||
        Type == TimerType.Amiodarona && TargetDuration.HasValue && Elapsed.TotalSeconds > TargetDuration.Value.TotalSeconds;

    partial void OnElapsedChanged(TimeSpan value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(IsOverThreshold));
    }
}
