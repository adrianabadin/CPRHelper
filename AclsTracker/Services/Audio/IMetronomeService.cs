namespace AclsTracker.Services.Audio;

/// <summary>
/// High-precision metronome engine using System.Diagnostics.Stopwatch (per D-01).
/// Must maintain ±1 BPM accuracy over extended sessions.
/// </summary>
public interface IMetronomeService
{
    /// <summary>Current BPM setting. Valid range: 100-120 (AHA ACLS 2020).</summary>
    int Bpm { get; set; }

    /// <summary>Whether the metronome is currently playing.</summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Fires on each beat. Subscribers must be thread-safe.
    /// Fired on main thread for UI synchronization.
    /// </summary>
    event Action? OnBeat;

    /// <summary>Start the metronome at current BPM.</summary>
    void Start();

    /// <summary>Stop the metronome.</summary>
    void Stop();

    /// <summary>Change BPM. Clamped to 100-120 range.</summary>
    void SetBpm(int bpm);
}