using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AclsTracker.Models;
using AclsTracker.Services.Database;
using AclsTracker.Services.EventLog;
using AclsTracker.Services.Export;

namespace AclsTracker.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly IEventLogService _eventLogService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPdfExportService _pdfExportService;
    private readonly ICsvExportService _csvExportService;

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

    // Export state
    [ObservableProperty]
    private bool _isExporting;

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

    public HistorialViewModel(
        IEventLogService eventLogService,
        ISessionRepository sessionRepository,
        IPdfExportService pdfExportService,
        ICsvExportService csvExportService)
    {
        _eventLogService = eventLogService;
        _sessionRepository = sessionRepository;
        _pdfExportService = pdfExportService;
        _csvExportService = csvExportService;
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

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportPdf()
    {
        if (SelectedSession is null) return;
        IsExporting = true;
        UpdateExportCommands();
        try
        {
            var events = SessionEvents.ToList();
            var filePath = await _pdfExportService.GeneratePdfAsync(SelectedSession, events);

            // Save local copy to app data
            var localDir = Path.Combine(FileSystem.Current.AppDataDirectory, "Exports");
            Directory.CreateDirectory(localDir);
            var localPath = Path.Combine(localDir, Path.GetFileName(filePath));
            File.Copy(filePath, localPath, overwrite: true);

            // Open share sheet
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar PDF — ACLS Tracker",
                File = new ShareFile(filePath),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS
                    ? new Rect(0, 0, 0, 0) : Rect.Zero
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo generar el PDF: {ex.Message}", "OK");
        }
        finally
        {
            IsExporting = false;
            UpdateExportCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportCsv()
    {
        if (SelectedSession is null) return;
        IsExporting = true;
        UpdateExportCommands();
        try
        {
            var events = SessionEvents.ToList();
            var filePath = await _csvExportService.GenerateCsvAsync(SelectedSession, events);

            // Save local copy
            var localDir = Path.Combine(FileSystem.Current.AppDataDirectory, "Exports");
            Directory.CreateDirectory(localDir);
            var localPath = Path.Combine(localDir, Path.GetFileName(filePath));
            File.Copy(filePath, localPath, overwrite: true);

            // Open share sheet
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar CSV — ACLS Tracker",
                File = new ShareFile(filePath),
                PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS
                    ? new Rect(0, 0, 0, 0) : Rect.Zero
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo generar el CSV: {ex.Message}", "OK");
        }
        finally
        {
            IsExporting = false;
            UpdateExportCommands();
        }
    }

    private bool CanExport() => !IsExporting && SelectedSession is not null;

    private void UpdateExportCommands()
    {
        ExportPdfCommand.NotifyCanExecuteChanged();
        ExportCsvCommand.NotifyCanExecuteChanged();
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
