using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Services.EventLog;
using AclsTracker.Services.Timer;

namespace AclsTracker.ViewModels;

public partial class EventRecordingViewModel : ObservableObject
{
    private readonly IEventLogService _eventLogService;
    private readonly ITimerService _timerService;

    [ObservableProperty]
    private CardiacRhythm _currentRhythm = CardiacRhythm.Ninguno;

    public string CurrentRhythmDisplay => GetRhythmDisplayName(CurrentRhythm);

    public bool IsRhythmRCE => CurrentRhythm == CardiacRhythm.RCE;
    public bool IsRhythmAESP => CurrentRhythm == CardiacRhythm.AESP;
    public bool IsRhythmAsistolia => CurrentRhythm == CardiacRhythm.Asistolia;
    public bool IsRhythmTV => CurrentRhythm == CardiacRhythm.TV;
    public bool IsRhythmFV => CurrentRhythm == CardiacRhythm.FV;

    partial void OnCurrentRhythmChanged(CardiacRhythm value)
    {
        OnPropertyChanged(nameof(CurrentRhythmDisplay));
        OnPropertyChanged(nameof(IsRhythmRCE));
        OnPropertyChanged(nameof(IsRhythmAESP));
        OnPropertyChanged(nameof(IsRhythmAsistolia));
        OnPropertyChanged(nameof(IsRhythmTV));
        OnPropertyChanged(nameof(IsRhythmFV));
    }

    [ObservableProperty]
    private bool _isSessionActive;

    public ObservableCollection<EventRecord> Events => _eventLogService.Events;

    public DateTime? SessionStartTime => _eventLogService.SessionStartTime;


    public ObservableCollection<HsAndTsItem> HsAndTsItems { get; } = new();

    public IEnumerable<HsAndTsItem> HItems => HsAndTsItems.Where(i => i.Category == "H");
    public IEnumerable<HsAndTsItem> TItems => HsAndTsItems.Where(i => i.Category == "T");

    public EventRecordingViewModel(IEventLogService eventLogService, ITimerService timerService)
    {
        _eventLogService = eventLogService;
        _timerService = timerService;
        InitializeHsAndTs();
    }

    private void InitializeHsAndTs()
    {
        // H's (Causas reversibles en español)
        HsAndTsItems.Add(new HsAndTsItem { Id = "h-hipovolemia",    Name = "Hipovolemia",                    Category = "H" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "h-hipoxia",        Name = "Hipoxia",                        Category = "H" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "h-hidrogeniones",  Name = "Hidrogeniones (Acidosis)",        Category = "H" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "h-potasio",        Name = "Hipo/Hiperpotasemia",             Category = "H" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "h-hipotermia",     Name = "Hipotermia",                     Category = "H" });

        // T's (Causas reversibles en español)
        HsAndTsItems.Add(new HsAndTsItem { Id = "t-neumo",          Name = "Neumotórax a Tensión",            Category = "T" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "t-taponamiento",   Name = "Taponamiento Cardíaco",           Category = "T" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "t-toxicos",        Name = "Tóxicos",                        Category = "T" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "t-trombosis-pulm", Name = "Trombosis Pulmonar",              Category = "T" });
        HsAndTsItems.Add(new HsAndTsItem { Id = "t-trombosis-cor",  Name = "Trombosis Coronaria",             Category = "T" });
    }

    [RelayCommand]
    private void SelectRhythm(CardiacRhythm rhythm)
    {
        CurrentRhythm = rhythm;
        var displayName = GetRhythmDisplayName(rhythm);
        _eventLogService.LogEvent("RhythmChange", $"Ritmo: {displayName}");
    }

    [RelayCommand]
    private void ToggleHsAndTsItem(HsAndTsItem item)
    {
        item.IsChecked = !item.IsChecked;
        item.CheckedAt = DateTime.Now;

        var action = item.IsDismissed ? "H&T descartado" : "H&T considerado";
        _eventLogService.LogEvent("HsTs", $"{action}: {item.Name}");
    }

    [RelayCommand]
    private void DismissHsAndTsItem(HsAndTsItem item)
    {
        item.IsDismissed = true;
        item.IsChecked = true;
        item.CheckedAt = DateTime.Now;
        _eventLogService.LogEvent("HsTs", $"H&T descartado: {item.Name}");
    }

    [RelayCommand]
    private void LogCustomEvent(string description)
    {
        _eventLogService.LogEvent("Custom", description);
    }

    [RelayCommand]
    private void StartRecording()
    {
        _eventLogService.StartSession();
        IsSessionActive = true;
    }

    [RelayCommand]
    private void StopRecording()
    {
        _eventLogService.EndSession();
        IsSessionActive = false;
    }

    private static string GetRhythmDisplayName(CardiacRhythm rhythm) => rhythm switch
    {
        CardiacRhythm.Ninguno   => "Ninguno",
        CardiacRhythm.RCE       => "RCE",
        CardiacRhythm.AESP      => "AESP",
        CardiacRhythm.Asistolia => "Asistolia",
        CardiacRhythm.TV        => "TV",
        CardiacRhythm.FV        => "FV",
        _                       => rhythm.ToString()
    };
}
