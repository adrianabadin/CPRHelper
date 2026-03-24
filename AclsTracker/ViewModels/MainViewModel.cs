using CommunityToolkit.Mvvm.ComponentModel;
using AclsTracker.ViewModels;

namespace AclsTracker.ViewModels;

/// <summary>
/// Composite ViewModel for MainPage. Exposes both MetronomeViewModel
/// and TimerViewModel as properties so the single-screen layout (D-04)
/// can bind to both.
/// </summary>
public class MainViewModel
{
    public MetronomeViewModel Metronome { get; }
    public TimerViewModel Timer { get; }

    public MainViewModel(MetronomeViewModel metronome, TimerViewModel timer)
    {
        Metronome = metronome;
        Timer = timer;
    }
}