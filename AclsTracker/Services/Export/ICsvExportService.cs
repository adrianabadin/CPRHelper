using AclsTracker.Models;

namespace AclsTracker.Services.Export;

/// <summary>
/// Generates CSV exports from ACLS session event data.
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Generates a CSV file for the given session events.
    /// </summary>
    /// <param name="session">The resuscitation session metadata.</param>
    /// <param name="events">All events recorded during the session.</param>
    /// <returns>Full file path of the generated CSV in the cache directory.</returns>
    Task<string> GenerateCsvAsync(Session session, List<EventRecord> events);
}
