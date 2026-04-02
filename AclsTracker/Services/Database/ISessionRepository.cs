using AclsTracker.Models;

namespace AclsTracker.Services.Database;

public interface ISessionRepository
{
    Task InitializeAsync();
    Task SaveSessionAsync(Session session, List<EventRecord> events);
    Task<List<Session>> SearchSessionsAsync(string? searchText, DateTime? fromDate, DateTime? toDate);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<List<EventRecord>> GetSessionEventsAsync(string sessionId);

    // ============ User-Scoped Operations ============

    /// <summary>Get all sessions without a UserId (orphan sessions).</summary>
    Task<List<Session>> GetOrphanSessionsAsync();

    /// <summary>Delete all sessions (and their events) for a specific user from local SQLite only.</summary>
    Task DeleteByUserIdAsync(string userId);

    /// <summary>Update the UserId field on an existing session (for claiming orphans).</summary>
    Task UpdateSessionUserIdAsync(string sessionId, string userId);

    /// <summary>Insert a downloaded session and its events. Skips if session ID already exists (immutable, no duplicates).</summary>
    Task InsertDownloadedSessionAsync(Session session, List<EventRecordEntity> events);

    /// <summary>Get all sessions that belong to a specific user.</summary>
    Task<List<Session>> GetSessionsByUserIdAsync(string userId);

    // ============ SyncQueue Operations ============

    /// <summary>Get pending sync items ready for retry (NextRetryAt &lt;= now), ordered by CreatedAt.</summary>
    Task<List<SyncQueueItem>> GetPendingSyncItemsAsync();

    /// <summary>Insert or replace a queue item for retry.</summary>
    Task EnqueueSyncItemAsync(SyncQueueItem item);

    /// <summary>Delete a queue item by its GUID.</summary>
    Task RemoveSyncItemAsync(string id);
}
