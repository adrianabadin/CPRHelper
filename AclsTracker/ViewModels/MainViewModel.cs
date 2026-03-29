using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.ViewModels;

namespace AclsTracker.ViewModels;

/// <summary>
/// Composite ViewModel for MainPage. Exposes MetronomeViewModel, TimerViewModel,
/// and EventRecordingViewModel as properties so the single-screen layout (D-04)
/// can bind to all three. Coordinates action commands and 2-minute pulse-check popup.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public MetronomeViewModel Metronome { get; }
    public TimerViewModel Timer { get; }
    public EventRecordingViewModel EventRecording { get; }

    private IDispatcherTimer? _pulseCheckTimer;
    private IDispatcherTimer? _chargingWarningTimer;
    private int _amiodaronaDoseCount;

    [ObservableProperty]
    private bool _isAmiodaronaEnabled;

    public MainViewModel(MetronomeViewModel metronome, TimerViewModel timer, EventRecordingViewModel eventRecording)
    {
        Metronome = metronome;
        Timer = timer;
        EventRecording = eventRecording;

        // Subscribe to rhythm changes to update Amiodarona enabled state and show defibrillation popup
        EventRecording.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EventRecordingViewModel.CurrentRhythm))
            {
                IsAmiodaronaEnabled = EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV;

                if (EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV)
                {
                    _ = Application.Current!.MainPage!
                        .DisplayAlert("Ritmo Desfibrilable", "Defibrile y reanude compresiones.", "ACEPTAR");
                    EventRecording.LogCustomEventCommand.Execute("Ritmo desfibrilable detectado");
                }
            }
        };
    }

    [RelayCommand]
    private void StartCode()
    {
        _amiodaronaDoseCount = 0;
        Timer.StartSessionCommand.Execute(null);
        EventRecording.StartRecordingCommand.Execute(null);

        // Start compressions timer immediately
        Timer.StartTimerCommand.Execute("compressions");

        // Start 2-minute countdown for pulse-check popup
        _pulseCheckTimer?.Stop();
        _pulseCheckTimer = Application.Current!.Dispatcher.CreateTimer();
        _pulseCheckTimer.Interval = TimeSpan.FromMinutes(2);
        _pulseCheckTimer.IsRepeating = false;
        _pulseCheckTimer.Tick += OnPulseCheckDue;
        _pulseCheckTimer.Start();

        // Charging warning 20s before pulse check (at 1:40)
        _chargingWarningTimer?.Stop();
        _chargingWarningTimer = Application.Current!.Dispatcher.CreateTimer();
        _chargingWarningTimer.Interval = TimeSpan.FromSeconds(100);
        _chargingWarningTimer.IsRepeating = false;
        _chargingWarningTimer.Tick += OnChargingWarning;
        _chargingWarningTimer.Start();
    }

    [RelayCommand]
    private void StopCode()
    {
        _pulseCheckTimer?.Stop();
        _pulseCheckTimer = null;
        _chargingWarningTimer?.Stop();
        _chargingWarningTimer = null;
        Timer.StopSessionCommand.Execute(null);
        EventRecording.StopRecordingCommand.Execute(null);
    }

    [RelayCommand]
    private void NewCycle()
    {
        Timer.NewCprCycleCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Nuevo ciclo RCP");

        // Reset 2-minute countdown
        _pulseCheckTimer?.Stop();
        _pulseCheckTimer = Application.Current!.Dispatcher.CreateTimer();
        _pulseCheckTimer.Interval = TimeSpan.FromMinutes(2);
        _pulseCheckTimer.IsRepeating = false;
        _pulseCheckTimer.Tick += OnPulseCheckDue;
        _pulseCheckTimer.Start();

        // Reset charging warning
        _chargingWarningTimer?.Stop();
        _chargingWarningTimer = Application.Current!.Dispatcher.CreateTimer();
        _chargingWarningTimer.Interval = TimeSpan.FromSeconds(100);
        _chargingWarningTimer.IsRepeating = false;
        _chargingWarningTimer.Tick += OnChargingWarning;
        _chargingWarningTimer.Start();
    }

    private async void OnChargingWarning(object? sender, EventArgs e)
    {
        _chargingWarningTimer?.Stop();
        var toast = CommunityToolkit.Maui.Alerts.Toast.Make(
            "⚡ Prepare desfibrilador — Check de pulso en 20s",
            CommunityToolkit.Maui.Core.ToastDuration.Long);
        await toast.Show();
    }

    private async void OnPulseCheckDue(object? sender, EventArgs e)
    {
        _pulseCheckTimer?.Stop();

        // Build medication suggestion lines based on ACLS 2020 protocol
        var suggestions = new List<string>();

        // ADRENALINA — ACLS: 1mg IV/IO cada 3-5 minutos para todos los ritmos de paro
        // Timer[3] (Adrenalina) resets when user presses ADRENALINA, so IsOverThreshold
        // means >4 min since last dose — correct protocol timing check.
        if (Timer.Timers[3].IsOverThreshold)
        {
            suggestions.Add("¿Hora de Adrenalina?");
        }

        // AMIODARONA — ACLS: para FV/TV refractaria, máx 2 dosis
        // 1ra dosis: 300mg tras 2do shock
        // 2da dosis: 150mg tras dosis adicional
        // Suggest only if: timer >4 min AND rhythm is shockable AND max doses not reached
        if (Timer.Timers[4].IsOverThreshold &&
            EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV &&
            _amiodaronaDoseCount < 2)
        {
            string doseHint = _amiodaronaDoseCount == 0
                ? "¿Hora de Amiodarona? (1ra dosis: 300mg)"
                : "¿Hora de Amiodarona? (2da dosis: 150mg)";
            suggestions.Add(doseHint);
        }

        // Build message with suggestions (if any)
        string message = "Han pasado 2 minutos.\nConstate pulso y ritmo.\nAdministre 2 ventilaciones.";

        if (suggestions.Count > 0)
        {
            message += "\n\n" + string.Join("\n", suggestions);
        }

        bool defibrilar = await Application.Current!.MainPage!
            .DisplayAlert("Check de Pulso", message, "DEFIBRILAR", "CONTINUAR");

        if (defibrilar) // DEFIBRILAR pressed (first button returns true)
        {
            EventRecording.LogCustomEventCommand.Execute("Defibrilación realizada");
        }
        else // CONTINUAR pressed (or dismissed)
        {
            // After CONTINUAR: pause compressions, start pulse-check timer
            Timer.PauseCompressions();
            Timer.StartPulseCheckTimer();
        }
    }

    [RelayCommand]
    private void Adrenalina()
    {
        Timer.MarkMedicationGivenCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Adrenalina administrada");
    }

    [RelayCommand]
    private void Amiodarona()
    {
        _amiodaronaDoseCount++;
        Timer.MarkAmiodaronaGivenCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Amiodarona administrada");
    }

    [RelayCommand]
    private void Defibrilar()
    {
        EventRecording.LogCustomEventCommand.Execute("Defibrilación realizada");
    }
}
