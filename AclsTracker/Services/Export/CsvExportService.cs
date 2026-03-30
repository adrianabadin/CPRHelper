using System.Text;
using AclsTracker.Models;

namespace AclsTracker.Services.Export;

/// <summary>
/// StreamWriter-based CSV generation for ACLS session event data.
/// Produces UTF-8 BOM encoded CSV with 6 columns for Spanish Excel compatibility.
/// </summary>
public class CsvExportService : ICsvExportService
{
    /// <inheritdoc/>
    public async Task<string> GenerateCsvAsync(Session session, List<EventRecord> events)
    {
        var fileName = $"ACLS_{session.PatientLastName}_{session.SessionStartTime:yyyyMMdd_HHmm}.csv";
        var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        // Order events by timestamp ascending
        var orderedEvents = events.OrderBy(e => e.Timestamp).ToList();

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        // UTF-8 with BOM for Spanish Excel compatibility
        using var writer = new StreamWriter(fileStream, new UTF8Encoding(true));

        // Header row
        await writer.WriteLineAsync("Timestamp,Tiempo_relativo,Tipo_evento,Descripcion,Detalles,Ritmo_actual");

        // Data rows
        foreach (var evt in orderedEvents)
        {
            var timestamp = evt.Timestamp.ToString("o");
            var relativeTime = $"{(int)evt.ElapsedSinceStart.TotalMinutes:D2}:{evt.ElapsedSinceStart.Seconds:D2}";
            var eventType = evt.EventType;
            var description = evt.Description;
            var details = evt.Details ?? string.Empty;
            // Ritmo_actual only populated for RhythmChange events
            var currentRhythm = evt.EventType == "RhythmChange" ? evt.Description : string.Empty;

            await writer.WriteLineAsync(
                $"{EscapeCsvField(timestamp)},{EscapeCsvField(relativeTime)},{EscapeCsvField(eventType)},{EscapeCsvField(description)},{EscapeCsvField(details)},{EscapeCsvField(currentRhythm)}");
        }

        await writer.FlushAsync();

        return filePath;
    }

    /// <summary>
    /// Escapes a field for CSV: wraps in double quotes if it contains commas, quotes, or newlines.
    /// Doubles any internal double quotes per RFC 4180.
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
