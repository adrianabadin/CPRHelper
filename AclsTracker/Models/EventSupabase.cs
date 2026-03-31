using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AclsTracker.Models;

/// <summary>
/// Supabase Postgrest model mapping to the remote events table.
/// Used for upload/download operations in the sync service.
/// </summary>
[Table("events")]
public class EventSupabase : BaseModel
{
    /// <summary>Unique event identifier (UUID).</summary>
    [PrimaryKey("id", false)]
    public string Id { get; set; } = string.Empty;

    /// <summary>ID of the session this event belongs to.</summary>
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>Absolute timestamp when the event occurred.</summary>
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>Elapsed ticks since session start (for relative timing).</summary>
    [Column("elapsed_ticks")]
    public long ElapsedTicks { get; set; }

    /// <summary>Event type identifier (e.g. "CPR_START", "DEFIB", "DRUG").</summary>
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Human-readable event description.</summary>
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional additional details (JSON or free text).</summary>
    [Column("details")]
    public string? Details { get; set; }
}
