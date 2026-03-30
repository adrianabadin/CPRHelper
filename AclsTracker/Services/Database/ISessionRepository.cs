using AclsTracker.Models;

namespace AclsTracker.Services.Database;

public interface ISessionRepository
{
    Task InitializeAsync();
    Task SaveSessionAsync(Session session, List<EventRecord> events);
    Task<List<Session>> SearchSessionsAsync(string? searchText, DateTime? fromDate, DateTime? toDate);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<List<EventRecord>> GetSessionEventsAsync(string sessionId);
}
