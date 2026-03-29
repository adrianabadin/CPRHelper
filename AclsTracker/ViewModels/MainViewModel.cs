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
    }

    [RelayCommand]
    private void StopCode()
    {
        _pulseCheckTimer?.Stop();
        _pulseCheckTimer = null;
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
    }

    private async void OnPulseCheckDue(object? sender, EventArgs e)
    {
        _pulseCheckTimer?.Stop();
        await Application.Current!.MainPage!
            .DisplayAlert("Check de Pulso", "Han pasado 2 minutos.\nConstate pulso y ritmo.\nAdministre 2 ventilaciones.", "ACEPTAR");
        // After ACEPTAR: pause compressions, start pulse-check timer
        Timer.PauseCompressions();
        Timer.StartPulseCheckTimer();
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
        Timer.MarkAmiodaronaGivenCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Amiodarona administrada");
    }

    [RelayCommand]
    private void Defibrilar()
    {
        EventRecording.LogCustomEventCommand.Execute("Defibrilación realizada");
    }
}
