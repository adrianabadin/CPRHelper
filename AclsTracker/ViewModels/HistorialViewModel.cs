using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Services.Database;
using AclsTracker.Services.EventLog;

namespace AclsTracker.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly IEventLogService _eventLogService;
    private readonly ISessionRepository _sessionRepository;

    // Expose live events from EventLogService
    public ObservableCollection<EventRecord> LiveEvents => _eventLogService.Events;

    // Saved sessions list
    [ObservableProperty]
    private ObservableCollection<Session> _savedSessions = new();

    // Selected session detail
    [ObservableProperty]
    private Session? _selectedSession;

    [ObservableProperty]
    private ObservableCollection<EventRecord> _sessionEvents = new();

    // Search state
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    // View toggle: 0 = live session, 1 = saved sessions list, 2 = session detail
    [ObservableProperty]
    private int _currentView = 0;

    // Computed property for view switching
    public bool IsLiveView => CurrentView == 0;
    public bool IsSavedView => CurrentView == 1;
    public bool IsDetailView => CurrentView == 2;

    partial void OnCurrentViewChanged(int value)
    {
        OnPropertyChanged(nameof(IsLiveView));
        OnPropertyChanged(nameof(IsSavedView));
        OnPropertyChanged(nameof(IsDetailView));
    }

    public HistorialViewModel(IEventLogService eventLogService, ISessionRepository sessionRepository)
    {
        _eventLogService = eventLogService;
        _sessionRepository = sessionRepository;
    }

    [RelayCommand]
    private async Task ShowSavedSessions()
    {
        CurrentView = 1;
        await LoadSavedSessions();
    }

    [RelayCommand]
    private void ShowLiveSession()
    {
        CurrentView = 0;
        SelectedSession = null;
        SessionEvents.Clear();
    }

    [RelayCommand]
    public async Task LoadSavedSessions()
    {
        var sessions = await _sessionRepository.SearchSessionsAsync(
            string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            FromDate,
            ToDate);
        SavedSessions = new ObservableCollection<Session>(sessions);
    }

    [RelayCommand]
    private async Task SearchSessions()
    {
        await LoadSavedSessions();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        FromDate = null;
        ToDate = null;
        _ = LoadSavedSessions();
    }

    [RelayCommand]
    private async Task SelectSession(Session? session)
    {
        if (session is null) return;
        SelectedSession = session;
        CurrentView = 2;

        var events = await _sessionRepository.GetSessionEventsAsync(session.Id);
        SessionEvents = new ObservableCollection<EventRecord>(events);
    }

    [RelayCommand]
    private void BackToList()
    {
        CurrentView = 1;
        SelectedSession = null;
        SessionEvents.Clear();
    }

    // Helper for session display
    public static string FormatSessionInfo(Session s) =>
        $"{s.PatientLastName}, {s.PatientName} — DNI: {s.PatientDNI} — {s.SessionStartTime:dd/MM/yyyy HH:mm}";

    public static string FormatSessionDuration(Session s)
    {
        var duration = s.SessionEndTime - s.SessionStartTime;
        return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
    }
}
