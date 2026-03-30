using AclsTracker.Models;

namespace AclsTracker.Services.Export;

/// <summary>
/// Generates PDF clinical reports from ACLS session data.
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Generates a PDF report for the given session and events.
    /// </summary>
    /// <param name="session">The resuscitation session metadata.</param>
    /// <param name="events">All events recorded during the session.</param>
    /// <returns>Full file path of the generated PDF in the cache directory.</returns>
    Task<string> GeneratePdfAsync(Session session, List<EventRecord> events);
}
