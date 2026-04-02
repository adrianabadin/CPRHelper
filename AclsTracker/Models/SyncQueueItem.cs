using SQLite;

namespace AclsTracker.Models;

/// <summary>
/// SQLite model for the persistent retry queue.
/// Stores serialized session/event data for re-upload after failures.
/// Survives app restarts since it's persisted in the local SQLite database.
/// </summary>
[Table("SyncQueue")]
public class SyncQueueItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    /// <summary>The session being synced.</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>JSON-serialized SessionSupabase data for re-upload.</summary>
    public string SessionData { get; set; } = string.Empty;

    /// <summary>JSON-serialized List&lt;EventSupabase&gt; data for re-upload.</summary>
    public string EventsData { get; set; } = string.Empty;

    /// <summary>Number of retry attempts so far.</summary>
    public int AttemptCount { get; set; }

    /// <summary>When first enqueued.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Next scheduled retry time for exponential backoff.</summary>
    public DateTime NextRetryAt { get; set; }
}
