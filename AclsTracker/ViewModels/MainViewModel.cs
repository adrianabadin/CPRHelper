using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.ViewModels;

namespace AclsTracker.ViewModels;

/// <summary>
/// Composite ViewModel for MainPage. Exposes both MetronomeViewModel
/// and TimerViewModel as properties so the single-screen layout (D-04)
/// can bind to both.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public MetronomeViewModel Metronome { get; }
    public TimerViewModel Timer { get; }
    public EventRecordingViewModel EventRecording { get; }

    public MainViewModel(MetronomeViewModel metronome, TimerViewModel timer, EventRecordingViewModel eventRecording)
    {
        Metronome = metronome;
        Timer = timer;
        EventRecording = eventRecording;
    }

    [RelayCommand]
    private void StartCode()
    {
        Timer.StartSessionCommand.Execute(null);
        EventRecording.StartRecordingCommand.Execute(null);
    }

    [RelayCommand]
    private void StopCode()
    {
        Timer.StopSessionCommand.Execute(null);
        EventRecording.StopRecordingCommand.Execute(null);
    }
}