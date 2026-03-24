namespace AclsTracker.Services.Audio;

/// <summary>
/// Cross-platform audio abstraction using Plugin.Maui.Audio (per D-02).
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Initialize audio resources (load click sound).
    /// Must be called before PlayClickAsync.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Play a single metronome click sound.
    /// Must complete fast enough for 120 BPM (< 500ms).
    /// </summary>
    Task PlayClickAsync();
}