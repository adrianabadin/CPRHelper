using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Services.Audio;

namespace AclsTracker.ViewModels;

/// <summary>
/// ViewModel for metronome control and visual beat display.
/// Exposes commands for start/stop and observable properties for
/// BPM and beat pulse animation trigger (per D-03: pulse animation).
/// </summary>
public partial class MetronomeViewModel : ObservableObject
{
    private readonly IMetronomeService _metronomeService;
    private bool _pausedForPulseCheck;

    [ObservableProperty]
    private int _bpm = 110;

    [ObservableProperty]
    private bool _isPlaying;

    /// <summary>
    /// Toggles on each beat to trigger pulse animation in the view (D-03).
    /// The view should animate a circle expansion whenever this changes.
    /// </summary>
    [ObservableProperty]
    private bool _beatPulse;

    /// <summary>
    /// Total beat count since last start. Useful for displaying compression count.
    /// </summary>
    [ObservableProperty]
    private int _beatCount;

    public MetronomeViewModel(IMetronomeService metronomeService)
    {
        _metronomeService = metronomeService;
        _metronomeService.OnBeat += HandleBeat;
    }

    /// <summary>
    /// Toggle metronome on/off. Updates IsPlaying state.
    /// </summary>
    [RelayCommand]
    private void ToggleMetronome()
    {
        if (IsPlaying)
        {
            _metronomeService.Stop();
            IsPlaying = false;
        }
        else
        {
            _metronomeService.SetBpm(Bpm);
            _metronomeService.Start();
            IsPlaying = true;
            BeatCount = 0;
        }
    }

    /// <summary>
    /// Increase BPM by 5, capped at 120.
    /// </summary>
    [RelayCommand]
    private void IncreaseBpm()
    {
        Bpm = Math.Min(Bpm + 5, 120);
        _metronomeService.SetBpm(Bpm);
    }

    /// <summary>
    /// Decrease BPM by 5, floored at 100.
    /// </summary>
    [RelayCommand]
    private void DecreaseBpm()
    {
        Bpm = Math.Max(Bpm - 5, 100);
        _metronomeService.SetBpm(Bpm);
    }

    /// <summary>
    /// Pause the metronome for a pulse check. Remembers it was paused so
    /// ResumeAfterPulseCheck can restore it automatically.
    /// </summary>
    public void PauseForPulseCheck()
    {
        if (IsPlaying)
        {
            _pausedForPulseCheck = true;
            _metronomeService.Stop();
            IsPlaying = false;
        }
    }

    /// <summary>
    /// Resume the metronome after a pulse check if it was paused by PauseForPulseCheck.
    /// </summary>
    public void ResumeAfterPulseCheck()
    {
        if (_pausedForPulseCheck)
        {
            _pausedForPulseCheck = false;
            _metronomeService.SetBpm(Bpm);
            _metronomeService.Start();
            IsPlaying = true;
        }
    }

    private void HandleBeat()
    {
        // Toggle BeatPulse to trigger animation on each beat (D-03)
        BeatPulse = !BeatPulse;
        BeatCount++;
    }
}
