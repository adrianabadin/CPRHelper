using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Views;

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

    private const int BannerDismissSeconds = 8;

    private IDispatcherTimer? _pulseCheckTimer;
    private IDispatcherTimer? _chargingWarningTimer;
    private int _amiodaronaDoseCount;
    private int _cycleCount;
    private bool _isPopupShowing;
    private bool _adrenalinaBannerFired;
    private bool _amiodaronaBannerFired;
    private bool _hasCompletedCode;

    /// <summary>
    /// Tracks number of defibrillations in current code session.
    /// Amiodarone is indicated only after 2+ shocks (refractory VF/pVT per AHA ACLS 2020).
    /// </summary>
    private int _defibrillationCount;

    /// <summary>
    /// Fired when the defibrillation command is executed.
    /// MainPage subscribes to trigger haptic feedback and visual animation.
    /// </summary>
    public event Action? DefibrillationTriggered;

    /// <summary>
    /// Fired when pulse check is due — signals MainPage to show the custom modal popup.
    /// Provides the list of suggestions to display in the popup.
    /// </summary>
    public event Action<List<string>>? PulseCheckRequired;

    [ObservableProperty]
    private bool _isAmiodaronaEnabled;

    private DateTime? _lastAdrenalinaTime;
    private DateTime? _lastAmiodaronaTime;

    [ObservableProperty]
    private bool _isAdrenalinaSuggested;

    [ObservableProperty]
    private bool _isAmiodaronaSuggested;

    public MainViewModel(MetronomeViewModel metronome, TimerViewModel timer, EventRecordingViewModel eventRecording)
    {
        Metronome = metronome;
        Timer = timer;
        EventRecording = eventRecording;

        // Subscribe to rhythm changes to update Amiodarona enabled state and show protocol guidance popup
        EventRecording.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EventRecordingViewModel.CurrentRhythm))
            {
                var rhythm = EventRecording.CurrentRhythm;
                IsAmiodaronaEnabled = rhythm is CardiacRhythm.TV or CardiacRhythm.FV;
                _ = HandleRhythmChangeAsync(rhythm); // fire-and-forget
                UpdateDrugSuggestions();
            }
        };

        // Monitor medication timer thresholds for >4min safety-net banner
        Timer.Timers[3].PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimerModel.IsOverThreshold)
                && Timer.Timers[3].IsOverThreshold && !_adrenalinaBannerFired)
            {
                _adrenalinaBannerFired = true;
                ShowNotification("💊 Hora de Adrenalina", BannerDismissSeconds);
            }
        };

        Timer.Timers[4].PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TimerModel.IsOverThreshold)
                && Timer.Timers[4].IsOverThreshold && !_amiodaronaBannerFired
                && EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV
                && _amiodaronaDoseCount < 2)
            {
                _amiodaronaBannerFired = true;
                ShowNotification("💊 Hora de Amiodarona", BannerDismissSeconds);
            }
        };
    }

    [RelayCommand]
    private async Task StartCode()
    {
        // If a code was previously completed, ask user to continue or start fresh
        if (_hasCompletedCode)
        {
            bool continuar = await Application.Current!.MainPage!
                .DisplayAlert("Codigo anterior",
                    "Desea continuar el codigo anterior o iniciar uno nuevo?",
                    "CONTINUAR", "NUEVO CODIGO");
            if (!continuar)
            {
                ResetCodeState();
            }
            _hasCompletedCode = false;
        }

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

    /// <summary>
    /// Resets all code session state — used when user chooses "NUEVO CODIGO" from the prompt.
    /// Clears cycle count, drug state, timers, and event log.
    /// </summary>
    private void ResetCodeState()
    {
        _cycleCount = 0;
        _amiodaronaDoseCount = 0;
        _defibrillationCount = 0;
        _lastAdrenalinaTime = null;
        _lastAmiodaronaTime = null;
        IsAdrenalinaSuggested = false;
        IsAmiodaronaSuggested = false;
        _adrenalinaBannerFired = false;
        _amiodaronaBannerFired = false;
        IsAmiodaronaEnabled = false;
        Timer.ResetAllCommand.Execute(null);
        EventRecording.StopRecordingCommand.Execute(null);
        EventRecording.StartRecordingCommand.Execute(null); // fresh recording
    }

    [RelayCommand]
    private void StopCode()
    {
        _pulseCheckTimer?.Stop();
        _pulseCheckTimer = null;
        _chargingWarningTimer?.Stop();
        _chargingWarningTimer = null;

        Timer.StopSessionCommand.Execute(null);
        // Do NOT reset state here — user may choose CONTINUAR on next StartCode
        _hasCompletedCode = true;
    }

    /// <summary> Fired when rhythm changes — signals MainPage to show rhythm modal popup. </summary>
    public event Action<string, string>? RhythmPopupRequired;

    private async Task HandleRhythmChangeAsync(CardiacRhythm newRhythm)
    {
        if (_isPopupShowing) return; // Prevent popup stacking on rapid rhythm changes
        _isPopupShowing = true;
        try
        {
            (string title, string message)? popup = newRhythm switch
            {
                CardiacRhythm.AESP      => ("Protocolo ACLS", "• Buscar causas reversibles\n• Valorar Causas Reversibles"),
                CardiacRhythm.Asistolia => ("Protocolo ACLS", "• Buscar causas reversibles\n• Valorar Causas Reversibles"),
                CardiacRhythm.TV        => ("Ritmo Desfibrilable", "• Ritmo desfibrilable. Preparar desfibrilador.\n• Valorar Causas Reversibles"),
                CardiacRhythm.FV        => ("Ritmo Desfibrilable", "• Ritmo desfibrilable. Preparar desfibrilador.\n• Valorar Causas Reversibles"),
                CardiacRhythm.RCE       => ("RCE Alcanzado", "RCE alcanzado\n• Mantener vía aérea y ventilación\n• Monitorear ritmo y presión arterial\n• Obtener ECG 12 derivaciones\n• Considerar objetivo temp 32-36°C\n• Valorar Causas Reversibles"),
                _ => null // Ninguno — no popup
            };

            if (popup is not null)
            {
                var message = popup.Value.message;

                // Add drug suggestion if protocol indicates
                var drugSuggestion = GetSuggestedDrug();
                if (drugSuggestion is not null)
                {
                    message += $"\n• 💊 {drugSuggestion.Drug} ({drugSuggestion.DoseHint})";
                }

                // Use modal popup instead of DisplayAlert — MainPage subscribes and pushes modal
                RhythmPopupRequired?.Invoke(popup.Value.title, message);

                string logEntry = newRhythm is CardiacRhythm.TV or CardiacRhythm.FV
                    ? $"Ritmo desfibrilable detectado: {newRhythm}"
                    : $"Ritmo detectado: {newRhythm}";
                EventRecording.LogCustomEventCommand.Execute(logEntry);
            }
        }
        finally
        {
            _isPopupShowing = false;
        }
    }

    private record DrugSuggestion(string Drug, string DoseHint);

    /// <summary>
    /// Central ACLS 2020 drug suggestion logic. Returns which single drug to suggest (mutually exclusive).
    /// Timer-based (≥3min since last dose) as source of truth. Only one drug highlighted at a time.
    /// </summary>
    private DrugSuggestion? GetSuggestedDrug()
    {
        var rhythm = EventRecording.CurrentRhythm;
        bool isNonShockable = rhythm is CardiacRhythm.AESP or CardiacRhythm.Asistolia;
        bool isShockable = rhythm is CardiacRhythm.TV or CardiacRhythm.FV;

        // RCE / Ninguno → no drugs
        if (!isNonShockable && !isShockable) return null;

        // First cycle: Adrenalina for non-shockable (ASAP), nothing for shockable (drugs start at 2nd pulse check)
        if (_cycleCount == 0)
        {
            return isNonShockable ? new DrugSuggestion("Adrenalina", "1mg") : null;
        }

        // AESP/Asistolia: only Adrenalina
        if (isNonShockable)
        {
            bool adrIndicated = _lastAdrenalinaTime == null || (DateTime.Now - _lastAdrenalinaTime.Value).TotalMinutes >= 3;
            return adrIndicated ? new DrugSuggestion("Adrenalina", "1mg") : null;
        }

        // TV/FV, cycleCount >= 1
        bool adrenalinaDue = _lastAdrenalinaTime == null || (DateTime.Now - _lastAdrenalinaTime.Value).TotalMinutes >= 3;
        // Amiodarone only after 2+ defibrillations (refractory VF/pVT per AHA ACLS 2020)
        bool amiodaronaDue = _defibrillationCount >= 2
                             && (_lastAmiodaronaTime == null || (DateTime.Now - _lastAmiodaronaTime.Value).TotalMinutes >= 3)
                             && _amiodaronaDoseCount < 2;

        // Both never given → Adrenalina first
        if (_lastAdrenalinaTime == null && _lastAmiodaronaTime == null)
        {
            return adrenalinaDue ? new DrugSuggestion("Adrenalina", "1mg") : null;
        }

        // Evaluate which is more urgent by time since last dose
        if (adrenalinaDue && amiodaronaDue)
        {
            // Both due: pick the one with more time elapsed (more urgent)
            var adrElapsed = _lastAdrenalinaTime == null ? TimeSpan.MaxValue : DateTime.Now - _lastAdrenalinaTime.Value;
            var amioElapsed = _lastAmiodaronaTime == null ? TimeSpan.MaxValue : DateTime.Now - _lastAmiodaronaTime.Value;

            if (adrElapsed >= amioElapsed)
                return new DrugSuggestion("Adrenalina", "1mg");

            string amioHint = _amiodaronaDoseCount == 0 ? "300mg" : "150mg";
            return new DrugSuggestion("Amiodarona", amioHint);
        }

        if (adrenalinaDue)
            return new DrugSuggestion("Adrenalina", "1mg");

        if (amiodaronaDue)
        {
            string amioHint = _amiodaronaDoseCount == 0 ? "300mg" : "150mg";
            return new DrugSuggestion("Amiodarona", amioHint);
        }

        return null;
    }

    /// <summary>
    /// Updates button highlights based on GetSuggestedDrug(). Mutually exclusive — only one red button.
    /// </summary>
    private void UpdateDrugSuggestions()
    {
        var suggestion = GetSuggestedDrug();
        IsAdrenalinaSuggested = suggestion?.Drug == "Adrenalina";
        IsAmiodaronaSuggested = suggestion?.Drug == "Amiodarona";
    }

    [RelayCommand]
    private void NewCycle()
    {
        _cycleCount++;
        Timer.NewCprCycleCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Nuevo ciclo RCP");
        ResumeAfterPulseCheck(); // Resume metronome if paused during pulse check

        UpdateDrugSuggestions();

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

    private void ShowNotification(string message, int autoDismissSeconds = BannerDismissSeconds)
    {
        // App uses Shell navigation, so MainPage is AppShell — navigate through Shell
        if (Shell.Current?.CurrentPage is Views.MainPage page)
        {
            page.ShowNotification(message, autoDismissSeconds);
        }
        else if (Application.Current?.MainPage is Views.MainPage directPage)
        {
            directPage.ShowNotification(message, autoDismissSeconds);
        }
    }

    /// <summary>
    /// Pause compressions and metronome for a pulse check. Called when user
    /// presses "CHEQUEANDO PULSO Y RITMO" in the pulse check modal.
    /// </summary>
    public void PauseForPulseCheck()
    {
        Timer.PauseCompressions();
        Metronome.PauseForPulseCheck();
        Timer.StartPulseCheckTimer();
        EventRecording.LogCustomEventCommand.Execute("Check de pulso iniciado");
    }

    /// <summary>
    /// Resume the metronome after a pulse check. Called when user presses
    /// "NUEVO CICLO" to resume compressions.
    /// </summary>
    public void ResumeAfterPulseCheck()
    {
        Metronome.ResumeAfterPulseCheck();
        // Compressions resume happens via NewCycle calling Timer.NewCprCycleCommand
    }

    private async void OnChargingWarning(object? sender, EventArgs e)
    {
        _chargingWarningTimer?.Stop();
        ShowNotification("⚡ Prepare desfibrilador — Check de pulso en 20s", BannerDismissSeconds);
    }

    private void OnPulseCheckDue(object? sender, EventArgs e)
    {
        _pulseCheckTimer?.Stop();

        var suggestions = new List<string>();

        // IV/IO access — first cycle only
        if (_cycleCount == 0)
        {
            suggestions.Add("¿Colocó acceso IV/IO?");
        }

        // Causas reversibles — for non-shockable rhythms
        var rhythm = EventRecording.CurrentRhythm;
        if (rhythm is CardiacRhythm.AESP or CardiacRhythm.Asistolia)
        {
            suggestions.Add("Valorar Causas Reversibles");
        }

        // Compressor rotation — every pulse check
        suggestions.Add("¿Rotar compresor?");

        // Drug suggestion — single drug from centralized ACLS protocol logic
        var drugSuggestion = GetSuggestedDrug();
        if (drugSuggestion is not null)
        {
            suggestions.Add($"💊 Administrar {drugSuggestion.Drug} ({drugSuggestion.DoseHint})");
        }

        // Fire event to show custom modal popup — MainPage subscribes and pushes PulseCheckPage
        PulseCheckRequired?.Invoke(suggestions);
    }

    [RelayCommand]
    private void Adrenalina()
    {
        _lastAdrenalinaTime = DateTime.Now;
        Timer.MarkMedicationGivenCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Adrenalina administrada");
        _adrenalinaBannerFired = false;
        UpdateDrugSuggestions();
    }

    [RelayCommand]
    private void Amiodarona()
    {
        _amiodaronaDoseCount++;
        _lastAmiodaronaTime = DateTime.Now;
        Timer.MarkAmiodaronaGivenCommand.Execute(null);
        EventRecording.LogCustomEventCommand.Execute("Amiodarona administrada");
        _amiodaronaBannerFired = false;
        UpdateDrugSuggestions();
    }

    [RelayCommand]
    private void Defibrilar()
    {
        _defibrillationCount++;
        DefibrillationTriggered?.Invoke();
        EventRecording.LogCustomEventCommand.Execute("Defibrilación realizada");
    }
}
