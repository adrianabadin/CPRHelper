using CommunityToolkit.Mvvm.ComponentModel;

namespace AclsTracker.Models;

public partial class EventRecord : ObservableObject
{
    public string Id { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }           // Exact time including milliseconds (REGI-02)
    public TimeSpan ElapsedSinceStart { get; init; }   // Time since session start for relative display
    public string EventType { get; init; } = string.Empty;   // "RhythmChange", "Medication", "CprCycle", "HsTs", "Custom"
    public string Description { get; init; } = string.Empty; // Human-readable description in Spanish
    public string? Details { get; init; }                     // Optional extra data
}
