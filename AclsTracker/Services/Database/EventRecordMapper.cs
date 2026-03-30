using AclsTracker.Models;

namespace AclsTracker.Services.Database;

public static class EventRecordMapper
{
    /// <summary>
    /// Maps a UI EventRecord to a DB EventRecordEntity.
    /// ElapsedSinceStart is stored as Ticks to preserve precision.
    /// </summary>
    public static EventRecordEntity ToEntity(this EventRecord record, string sessionId)
    {
        return new EventRecordEntity
        {
            Id = record.Id,
            SessionId = sessionId,
            Timestamp = record.Timestamp,
            ElapsedTicks = record.ElapsedSinceStart.Ticks,
            EventType = record.EventType,
            Description = record.Description,
            Details = record.Details,
        };
    }

    /// <summary>
    /// Maps a DB EventRecordEntity back to a UI EventRecord.
    /// ElapsedTicks is reconstructed into TimeSpan via FromTicks.
    /// </summary>
    public static EventRecord ToModel(this EventRecordEntity entity)
    {
        return new EventRecord
        {
            Id = entity.Id,
            Timestamp = entity.Timestamp,
            ElapsedSinceStart = TimeSpan.FromTicks(entity.ElapsedTicks),
            EventType = entity.EventType,
            Description = entity.Description,
            Details = entity.Details,
        };
    }
}
