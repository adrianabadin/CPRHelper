using System.Collections.ObjectModel;
using AclsTracker.Models;

namespace AclsTracker.Services.EventLog;

public class EventLogService : IEventLogService
{
    private readonly object _lock = new();

    public ObservableCollection<EventRecord> Events { get; } = new();

    public DateTime? SessionStartTime { get; private set; }

    public void StartSession()
    {
        SessionStartTime = DateTime.Now;
        LogEvent("Session", "Inicio de código");
    }

    public void EndSession()
    {
        LogEvent("Session", "Fin de código");
    }

    public EventRecord LogEvent(string eventType, string description, string? details = null)
    {
        var now = DateTime.Now;
        var elapsed = SessionStartTime.HasValue
            ? now - SessionStartTime.Value
            : TimeSpan.Zero;

        var record = new EventRecord
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = now,                   // Millisecond precision (REGI-02)
            ElapsedSinceStart = elapsed,
            EventType = eventType,
            Description = description,
            Details = details
        };

        lock (_lock)
        {
            Events.Insert(0, record);          // Newest first
        }

        return record;
    }

    public void ClearEvents()
    {
        lock (_lock)
        {
            Events.Clear();
        }
        SessionStartTime = null;
    }
}
