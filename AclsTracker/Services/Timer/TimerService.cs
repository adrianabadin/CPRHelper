using System.Collections.ObjectModel;
using System.Diagnostics;
using AclsTracker.Models;

namespace AclsTracker.Services.Timer;

/// <summary>
/// Manages multiple concurrent timers with independent controls (per D-04, TIME-01, TIME-02).
///
/// Architecture:
/// - Each timer has its own Stopwatch for drift-free elapsed tracking
/// - A single UI update loop runs at ~20Hz (50ms) to update all timer Elapsed properties
/// - ObservableCollection notifies the UI layer via data binding (TIME-03)
/// - Timers accumulate elapsed time across pause/resume cycles
///
/// Why single update loop instead of per-timer loops:
/// - Fewer threads/timers = less contention
/// - Consistent UI update rate across all timers
/// - Easier to coordinate StartAll/PauseAll
/// </summary>
public class TimerService : ITimerService, IDisposable
{
    private readonly ObservableCollection<TimerModel> _timers = new();
    private readonly Dictionary<string, Stopwatch> _stopwatches = new();
    private readonly Dictionary<string, TimeSpan> _accumulatedTime = new();
    private readonly object _lock = new();
    private IDispatcherTimer? _updateTimer;
    private bool _isDisposed;

    public ObservableCollection<TimerModel> Timers => _timers;

    public TimerModel AddTimer(string id, string name, TimerType type, TimeSpan? targetDuration = null)
    {
        lock (_lock)
        {
            // Prevent duplicate ids
            if (_stopwatches.ContainsKey(id))
                throw new ArgumentException($"Timer with id '{id}' already exists.", nameof(id));

            var model = new TimerModel
            {
                Id = id,
                Name = name,
                Type = type,
                TargetDuration = targetDuration,
                Elapsed = TimeSpan.Zero,
                IsRunning = false
            };

            _stopwatches[id] = new Stopwatch();
            _accumulatedTime[id] = TimeSpan.Zero;
            _timers.Add(model);

            EnsureUpdateLoopRunning();

            return model;
        }
    }

    public void RemoveTimer(string id)
    {
        lock (_lock)
        {
            var model = FindTimer(id);
            if (model is null) return;

            _stopwatches[id].Stop();
            _stopwatches.Remove(id);
            _accumulatedTime.Remove(id);
            _timers.Remove(model);
        }
    }

    public void StartTimer(string id)
    {
        lock (_lock)
        {
            var model = FindTimer(id);
            if (model is null || model.IsRunning) return;

            _stopwatches[id].Start();
            model.IsRunning = true;
        }
    }

    public void PauseTimer(string id)
    {
        lock (_lock)
        {
            var model = FindTimer(id);
            if (model is null || !model.IsRunning) return;

            _stopwatches[id].Stop();
            // Accumulate elapsed time so far for resume
            _accumulatedTime[id] += _stopwatches[id].Elapsed;
            _stopwatches[id].Reset();
            model.IsRunning = false;
        }
    }

    public void ResetTimer(string id)
    {
        lock (_lock)
        {
            var model = FindTimer(id);
            if (model is null) return;

            _stopwatches[id].Reset();
            _accumulatedTime[id] = TimeSpan.Zero;
            model.Elapsed = TimeSpan.Zero;
            model.IsRunning = false;
        }
    }

    public void StartAll()
    {
        lock (_lock)
        {
            foreach (var timer in _timers)
            {
                if (!timer.IsRunning)
                {
                    _stopwatches[timer.Id].Start();
                    timer.IsRunning = true;
                }
            }
        }
    }

    public void PauseAll()
    {
        lock (_lock)
        {
            foreach (var timer in _timers)
            {
                if (timer.IsRunning)
                {
                    _stopwatches[timer.Id].Stop();
                    _accumulatedTime[timer.Id] += _stopwatches[timer.Id].Elapsed;
                    _stopwatches[timer.Id].Reset();
                    timer.IsRunning = false;
                }
            }
        }
    }

    public void ResetAll()
    {
        lock (_lock)
        {
            foreach (var timer in _timers)
            {
                _stopwatches[timer.Id].Reset();
                _accumulatedTime[timer.Id] = TimeSpan.Zero;
                timer.Elapsed = TimeSpan.Zero;
                timer.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// Updates all timer Elapsed values from their Stopwatch readings.
    /// Runs on the main thread via IDispatcherTimer at ~20Hz (50ms interval).
    /// This drives the real-time display (TIME-03) and progress percentage (D-05).
    /// </summary>
    private void UpdateTimerValues(object? sender, EventArgs e)
    {
        lock (_lock)
        {
            foreach (var timer in _timers)
            {
                if (timer.IsRunning && _stopwatches.TryGetValue(timer.Id, out var sw))
                {
                    timer.Elapsed = _accumulatedTime[timer.Id] + sw.Elapsed;
                }
            }
        }
    }

    private void EnsureUpdateLoopRunning()
    {
        if (_updateTimer is not null) return;

        // Create dispatcher timer on main thread for UI-safe updates
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _updateTimer = Application.Current?.Dispatcher.CreateTimer();
            if (_updateTimer is not null)
            {
                _updateTimer.Interval = TimeSpan.FromMilliseconds(50); // ~20Hz update rate
                _updateTimer.Tick += UpdateTimerValues;
                _updateTimer.Start();
            }
        });
    }

    private TimerModel? FindTimer(string id)
    {
        return _timers.FirstOrDefault(t => t.Id == id);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _updateTimer?.Stop();
        _updateTimer = null;

        lock (_lock)
        {
            foreach (var sw in _stopwatches.Values)
                sw.Stop();

            _stopwatches.Clear();
            _accumulatedTime.Clear();
            _timers.Clear();
        }
    }
}
