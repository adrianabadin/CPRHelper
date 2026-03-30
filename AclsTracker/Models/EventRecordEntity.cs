using SQLite;

namespace AclsTracker.Models;

[Table("EventRecords")]
public class EventRecordEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string SessionId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    /// <summary>
    /// TimeSpan stored as ticks (long) to avoid SQLite serialization issues.
    /// Reconstructed via TimeSpan.FromTicks(ElapsedTicks).
    /// </summary>
    public long ElapsedTicks { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
}
