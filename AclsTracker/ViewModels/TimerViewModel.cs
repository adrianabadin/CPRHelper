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
/// - Epinephrine (medication administration every 3-5 min)
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
    /// Called once on ViewModel creation.
    /// </summary>
    private void InitializeDefaultTimers()
    {
        // Total elapsed time — no target, counts up indefinitely
        _timerService.AddTimer(
            id: "total-elapsed",
            name: "Tiempo Total",
            type: TimerType.TotalElapsed,
            targetDuration: null
        );

        // CPR cycle timer — 2-minute target with progress circle (D-05)
        _timerService.AddTimer(
            id: "cpr-cycle",
            name: "Ciclo RCP",
            type: TimerType.CprCycle,
            targetDuration: TimeSpan.FromMinutes(2)
        );

        // Compression set timer — tracks current compression set duration
        _timerService.AddTimer(
            id: "compressions",
            name: "Compresiones",
            type: TimerType.Compressions,
            targetDuration: null
        );

        // Epinephrine timer — 3-5 minute medication cycle
        _timerService.AddTimer(
            id: "epinephrine",
            name: "Adrenalina",
            type: TimerType.Medication,
            targetDuration: TimeSpan.FromMinutes(3)
        );
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
    /// </summary>
    [RelayCommand]
    private void NewCprCycle()
    {
        _timerService.ResetTimer("cpr-cycle");
        _timerService.StartTimer("cpr-cycle");
        _timerService.ResetTimer("compressions");
        _timerService.StartTimer("compressions");
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
}