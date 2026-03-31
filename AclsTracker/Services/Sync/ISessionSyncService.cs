using AclsTracker.Models;

namespace AclsTracker.Services.Sync;

/// <summary>
/// Orchestrates session sync between local SQLite and Supabase.
/// Listens to AuthStateChanged for automatic claim/download/cleanup triggers.
/// </summary>
public interface ISessionSyncService
{
    /// <summary>Upload a session (and its events) to Supabase. Queues on failure.</summary>
    Task UploadSessionAsync(Session session, List<EventRecord> events);

    /// <summary>Delete all sessions for the given user from local SQLite only.</summary>
    Task DeleteLocalUserSessionsAsync(string userId);

    /// <summary>Event fired when sync operations complete (for UI refresh).</summary>
    event EventHandler? SyncCompleted;
}
