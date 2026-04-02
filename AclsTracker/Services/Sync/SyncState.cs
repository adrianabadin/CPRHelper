namespace AclsTracker.Services.Sync;

/// <summary>
/// Sync state for the UI sync indicator.
/// </summary>
public enum SyncState
{
    /// <summary>WebSocket connected, all pending items processed (green indicator).</summary>
    Synced,

    /// <summary>Processing retry queue or downloading (yellow indicator).</summary>
    Syncing,

    /// <summary>No network or WebSocket disconnected (gray indicator).</summary>
    Offline
}
