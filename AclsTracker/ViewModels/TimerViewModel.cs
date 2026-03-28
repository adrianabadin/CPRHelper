using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Services.Timer;

namespace AclsTracker.ViewModels;

/// <summary>
/// ViewModel for the timer display panel (per D-04: all timers visible on same screen).
/// Exposes the timer collection and per-timer control commands.
///
/// Pre-creates the standard ACLS timer set on initialization:
/// - Total Elapsed (code duration since start)
/// - CPR Cycle (2-minute countdown cycles)
/// - Compressions (current set duration)
/// - Epinephrine/Adrenalina (medication administration every 3 min)
/// - Amiodarona (medication timer every 5 min)
/// - Pulse Check (time between pulse checks - turns red > 10s)
/// </summary>
public partial class TimerViewModel : ObservableObject
{
    private readonly ITimerService _timerService;

    /// <summary>
    /// Observable collection bound to the UI timer list (D-04: all visible on same screen).
    /// </summary>
    public ObservableCollection<TimerModel> Timers => _timerService.Timers;

    [ObservableProperty]
    private bool _isSessionActive;

    public TimerViewModel(ITimerService timerService)
    {
        _timerService = timerService;
        InitializeDefaultTimers();
    }

    /// <summary>
    /// Creates the standard timer set for ACLS resuscitation.
    /// Called once on ViewModel creation. Order matters for XAML binding (2x3 grid).
    /// </summary>
    private void InitializeDefaultTimers()
    {
        _timerService.AddTimer("total-elapsed",  "Tiempo Total",  TimerType.TotalElapsed,  null);
        _timerService.AddTimer("cpr-cycle",      "Ciclo RCP",     TimerType.CprCycle,      TimeSpan.FromMinutes(2));
        _timerService.AddTimer("compressions",   "Compresiones",  TimerType.Compressions,  null);
        _timerService.AddTimer("epinephrine",    "Adrenalina",    TimerType.Medication,    TimeSpan.FromMinutes(3));
        _timerService.AddTimer("amiodarona",     "Amiodarona",    TimerType.Amiodarona,    TimeSpan.FromMinutes(5));
        _timerService.AddTimer("pulse-check",    "T. Pulsos",     TimerType.PulseCheck,    null);
    }

    /// <summary>
    /// Start the full ACLS session — starts total elapsed + CPR cycle.
    /// Individual timers (compressions, medication) started by user action.
    /// </summary>
    [RelayCommand]
    private void StartSession()
    {
        _timerService.StartTimer("total-elapsed");
        _timerService.StartTimer("cpr-cycle");
        IsSessionActive = true;
    }

    /// <summary>
    /// Stop all timers (end of code).
    /// </summary>
    [RelayCommand]
    private void StopSession()
    {
        _timerService.PauseAll();
        IsSessionActive = false;
    }

    /// <summary>
    /// Start a specific timer by id. Used by per-timer UI buttons (TIME-02).
    /// </summary>
    [RelayCommand]
    private void StartTimer(string timerId)
    {
        _timerService.StartTimer(timerId);
    }

    /// <summary>
    /// Pause a specific timer by id (TIME-02).
    /// </summary>
    [RelayCommand]
    private void PauseTimer(string timerId)
    {
        _timerService.PauseTimer(timerId);
    }

    /// <summary>
    /// Reset a specific timer to zero (TIME-02).
    /// Useful for restarting CPR cycles or compression sets.
    /// </summary>
    [RelayCommand]
    private void ResetTimer(string timerId)
    {
        _timerService.ResetTimer(timerId);
    }

    /// <summary>
    /// Reset and restart CPR cycle timer — convenience for the common
    /// "new cycle" action after rhythm check every 2 minutes.
    /// Resumes compressions timer (does NOT reset — accumulates for FCT).
    /// Resets pulse-check timer.
    /// </summary>
    [RelayCommand]
    private void NewCprCycle()
    {
        _timerService.ResetTimer("cpr-cycle");
        _timerService.StartTimer("cpr-cycle");
        // Compressions timer intentionally NOT reset — accumulates total compression time for FCT
        _timerService.StartTimer("compressions");  // Resume compressions (may have been paused during pulse-check)
        // Reset pulse-check timer
        _timerService.PauseTimer("pulse-check");
        _timerService.ResetTimer("pulse-check");
    }

    /// <summary>
    /// Mark epinephrine administered — reset medication timer and restart.
    /// </summary>
    [RelayCommand]
    private void MarkMedicationGiven()
    {
        _timerService.ResetTimer("epinephrine");
        _timerService.StartTimer("epinephrine");
    }

    /// <summary>
    /// Mark amiodarona administered — reset amiodarona timer and restart.
    /// </summary>
    [RelayCommand]
    private void MarkAmiodaronaGiven()
    {
        _timerService.ResetTimer("amiodarona");
        _timerService.StartTimer("amiodarona");
    }

    /// <summary>
    /// Pause the compressions timer (used when checking pulse).
    /// </summary>
    public void PauseCompressions()
    {
        _timerService.PauseTimer("compressions");
    }

    /// <summary>
    /// Resume the compressions timer (used after pulse check).
    /// </summary>
    public void ResumeCompressions()
    {
        _timerService.StartTimer("compressions");
    }

    /// <summary>
    /// Start the pulse-check timer from zero.
    /// Call when compressions are paused to check for pulse.
    /// </summary>
    public void StartPulseCheckTimer()
    {
        _timerService.ResetTimer("pulse-check");
        _timerService.StartTimer("pulse-check");
    }

    /// <summary>
    /// Reset the pulse-check timer back to zero and stop it.
    /// </summary>
    public void ResetPulseCheckTimer()
    {
        _timerService.PauseTimer("pulse-check");
        _timerService.ResetTimer("pulse-check");
    }
}
