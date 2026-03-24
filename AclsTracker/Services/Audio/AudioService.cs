using Plugin.Maui.Audio;

namespace AclsTracker.Services.Audio;

/// <summary>
/// Cross-platform audio service wrapping Plugin.Maui.Audio (per D-02).
/// Pre-loads click sound for minimal latency during metronome playback.
/// </summary>
public class AudioService : IAudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _clickPlayer;
    private bool _isInitialized;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        // Pre-load the click sound into memory for instant playback
        var stream = await FileSystem.OpenAppPackageFileAsync("click.wav");
        _clickPlayer = _audioManager.CreatePlayer(stream);
        _isInitialized = true;
    }

    public async Task PlayClickAsync()
    {
        if (!_isInitialized || _clickPlayer is null)
        {
            await InitializeAsync();
        }

        // Stop previous playback and replay from beginning
        // This ensures clean audio even at 120 BPM (500ms interval)
        _clickPlayer!.Stop();
        _clickPlayer.Seek(0);
        _clickPlayer.Play();
    }

    public void Dispose()
    {
        _clickPlayer?.Dispose();
        _clickPlayer = null;
        _isInitialized = false;
    }
}
