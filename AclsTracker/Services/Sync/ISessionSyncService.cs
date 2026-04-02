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

    // ============ Realtime Sync ============

    /// <summary>Start Supabase Realtime subscription for session changes by the current user. Call after login.</summary>
    Task StartRealtimeSyncAsync(string userId);

    /// <summary>Stop Realtime subscription. Call before logout.</summary>
    void StopRealtimeSync();

    /// <summary>Current sync state (Synced, Syncing, Offline). Bindable for UI indicators.</summary>
    SyncState CurrentSyncState { get; }

    /// <summary>Event fired when sync state changes (for UI indicator updates).</summary>
    event EventHandler<SyncState>? SyncStateChanged;

    /// <summary>Event fired when sessions are downloaded from another device (for toast notifications).</summary>
    event EventHandler<int>? SessionsDownloaded;
}
