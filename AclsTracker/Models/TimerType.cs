namespace AclsTracker.Models;

public enum TimerType
{
    CprCycle,        // 2-minute CPR cycle timer
    Medication,      // Drug administration timer (e.g., epinephrine every 3-5 min)
    TotalElapsed,    // Total code duration since start
    Compressions     // Current compression set duration
}