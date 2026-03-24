using System.Collections.ObjectModel;
using AclsTracker.Models;

namespace AclsTracker.Services.Timer;

/// <summary>
/// Manages multiple independent timers visible on the same screen (per D-04).
/// Each timer can be started, paused, and reset independently (TIME-02).
/// Updates elapsed time in real-time at ~20Hz for smooth UI updates (TIME-03).
/// </summary>
public interface ITimerService
{
    /// <summary>Observable collection of all active timers for UI binding.</summary>
    ObservableCollection<TimerModel> Timers { get; }

    /// <summary>
    /// Add a new timer. Returns the created TimerModel for reference.
    /// </summary>
    /// <param name="id">Unique identifier (e.g., "cpr-cycle", "epinephrine")</param>
    /// <param name="name">Display name (e.g., "Ciclo RCP", "Adrenalina")</param>
    /// <param name="type">Timer type from TimerType enum</param>
    /// <param name="targetDuration">Optional target for progress display (e.g., 2 min)</param>
    TimerModel AddTimer(string id, string name, TimerType type, TimeSpan? targetDuration = null);

    /// <summary>Remove a timer by id.</summary>
    void RemoveTimer(string id);

    /// <summary>Start counting on a specific timer (TIME-02).</summary>
    void StartTimer(string id);

    /// <summary>Pause a specific timer, preserving elapsed time (TIME-02).</summary>
    void PauseTimer(string id);

    /// <summary>Reset a specific timer to zero (TIME-02).</summary>
    void ResetTimer(string id);

    /// <summary>Start all timers at once.</summary>
    void StartAll();

    /// <summary>Pause all timers at once.</summary>
    void PauseAll();

    /// <summary>Reset all timers at once.</summary>
    void ResetAll();
}