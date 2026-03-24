using System.Diagnostics;

namespace AclsTracker.Services.Audio;

/// <summary>
/// High-precision metronome using System.Diagnostics.Stopwatch (per D-01).
///
/// Timing strategy: Uses a dedicated background thread with Stopwatch
/// instead of System.Timers.Timer or Task.Delay to avoid timer drift.
/// Stopwatch provides microsecond precision on most platforms.
///
/// The loop calculates the exact next beat time and spin-waits briefly
/// before the target to compensate for thread scheduling jitter.
///
/// Pitfall mitigation (from PITFALLS.md):
/// - No Thread.Sleep for timing (too imprecise)
/// - Stopwatch for drift-free intervals
/// - OnBeat dispatched to main thread for safe UI updates
/// </summary>
public class MetronomeService : IMetronomeService
{
    private readonly IAudioService _audioService;
    private int _bpm = 110; // Default CPR rate (middle of 100-120 range)
    private bool _isPlaying;
    private CancellationTokenSource? _cts;
    private Thread? _metronomeThread;

    public int Bpm
    {
        get => _bpm;
        set => SetBpm(value);
    }

    public bool IsPlaying => _isPlaying;

    public event Action? OnBeat;

    public MetronomeService(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public void Start()
    {
        if (_isPlaying) return;

        _isPlaying = true;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _metronomeThread = new Thread(() => MetronomeLoop(token))
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal,
            Name = "MetronomeThread"
        };
        _metronomeThread.Start();
    }

    public void Stop()
    {
        if (!_isPlaying) return;

        _isPlaying = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    public void SetBpm(int bpm)
    {
        // Clamp to AHA ACLS 2020 valid range: 100-120 BPM (AUDI-01)
        _bpm = Math.Clamp(bpm, 100, 120);
    }

    private void MetronomeLoop(CancellationToken token)
    {
        var stopwatch = Stopwatch.StartNew();
        long nextBeatTicks = 0;

        // Initialize audio on first run
        _audioService.InitializeAsync().GetAwaiter().GetResult();

        while (!token.IsCancellationRequested)
        {
            long intervalTicks = Stopwatch.Frequency / _bpm * 60;

            // Coarse wait: sleep until close to next beat (saves CPU)
            long remainingMs = (nextBeatTicks - stopwatch.ElapsedTicks) * 1000 / Stopwatch.Frequency;
            if (remainingMs > 2)
            {
                Thread.Sleep((int)(remainingMs - 1));
            }

            // Fine wait: spin-wait for precise timing
            while (stopwatch.ElapsedTicks < nextBeatTicks && !token.IsCancellationRequested)
            {
                Thread.SpinWait(10);
            }

            if (token.IsCancellationRequested) break;

            // Fire beat
            _ = _audioService.PlayClickAsync();

            // Dispatch OnBeat to main thread for UI sync (AUDI-02)
            MainThread.BeginInvokeOnMainThread(() => OnBeat?.Invoke());

            // Schedule next beat relative to ideal time (prevents drift)
            nextBeatTicks += intervalTicks;

            // If we fell behind by more than one interval, reset
            if (stopwatch.ElapsedTicks - nextBeatTicks > intervalTicks)
            {
                nextBeatTicks = stopwatch.ElapsedTicks + intervalTicks;
            }
        }
    }
}
