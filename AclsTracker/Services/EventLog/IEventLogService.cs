using System.Collections.ObjectModel;
using AclsTracker.Models;

namespace AclsTracker.Services.EventLog;

public interface IEventLogService
{
    ObservableCollection<EventRecord> Events { get; }
    DateTime? SessionStartTime { get; }
    void StartSession();         // Records session start, sets SessionStartTime
    void EndSession();           // Records session end
    EventRecord LogEvent(string eventType, string description, string? details = null);
    void ClearEvents();          // Reset for new session
}
