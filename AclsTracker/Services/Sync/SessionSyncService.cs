using System.Diagnostics;
using AclsTracker.Models;
using AclsTracker.Services.Auth;
using AclsTracker.Services.Database;

namespace AclsTracker.Services.Sync;

/// <summary>
/// Core orchestration layer that coordinates upload, download, claim, and cleanup
/// between local SQLite and Supabase. Subscribes to auth state changes for automatic triggers.
/// </summary>
public class SessionSyncService : ISessionSyncService
{
    private readonly IAuthService _authService;
    private readonly ISessionRepository _sessionRepo;
    private readonly Supabase.Client _supabase;

    // Serializes concurrent login sync attempts (Supabase SDK may fire AuthStateChanged
    // multiple times for a single login event; the semaphore ensures only one
    // HandleLoginSyncAsync runs at a time, eliminating TOCTOU races in the download path).
    private readonly SemaphoreSlim _loginSyncLock = new(1, 1);

    // Retry queue for failed uploads
    private readonly Queue<(Func<Task> Operation, int AttemptCount)> _uploadQueue = new();
    private IDispatcherTimer? _retryTimer;
    private int _retryIntervalSeconds = 30;
    private const int MaxRetryAttempts = 5;
    private const int MaxRetryIntervalSeconds = 300; // 5 minutes

    public event EventHandler? SyncCompleted;
    public event EventHandler<SyncState>? SyncStateChanged;
    public event EventHandler<int>? SessionsDownloaded;

    public SyncState CurrentSyncState { get; private set; } = SyncState.Offline;

    public SessionSyncService(
        IAuthService authService,
        ISessionRepository sessionRepo,
        Supabase.Client supabase)
    {
        _authService = authService;
        _sessionRepo = sessionRepo;
        _supabase = supabase;

        // Subscribe to auth state changes for automatic triggers
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    // ============ Public Interface ============

    public async Task UploadSessionAsync(Session session, List<EventRecord> events)
    {
        try
        {
            await UploadSessionInternalAsync(session, events).ConfigureAwait(false);
            Debug.WriteLine($"[SessionSyncService] Upload succeeded for session {session.Id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Upload failed for session {session.Id}: {ex.Message}. Queuing for retry.");
            EnqueueRetry(async () => await UploadSessionInternalAsync(session, events).ConfigureAwait(false));
        }
    }

    public async Task DeleteLocalUserSessionsAsync(string userId)
    {
        try
        {
            await _sessionRepo.DeleteByUserIdAsync(userId).ConfigureAwait(false);
            Debug.WriteLine($"[SessionSyncService] Deleted local sessions for user {userId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Failed to delete local sessions for user {userId}: {ex.Message}");
        }
    }

    // ============ Realtime Sync (Stubs — Plan 02 implements) ============

    public Task StartRealtimeSyncAsync(string userId)
    {
        throw new NotImplementedException("Realtime sync will be implemented in Plan 02.");
    }

    public void StopRealtimeSync()
    {
        throw new NotImplementedException("Realtime sync will be implemented in Plan 02.");
    }

    // ============ Auth State Handler ============

    private async void OnAuthStateChanged(object? sender, bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            var userId = _authService.CurrentUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                await HandleLoginSyncAsync(userId).ConfigureAwait(false);
            }
        }
        // Note: logout cleanup (DeleteLocalUserSessionsAsync) is triggered from AuthViewModel
        // which retains the userId before signing out. This handler does NOT handle cleanup
        // because userId is gone after logout completes.
    }

    // ============ Login Sync Orchestration ============

    private async Task HandleLoginSyncAsync(string userId)
    {
        // Prevent concurrent login syncs (Supabase SDK can fire AuthStateChanged
        // multiple times for a single login). If a sync is already running, skip
        // the duplicate — the in-flight sync will handle everything.
        if (!await _loginSyncLock.WaitAsync(0).ConfigureAwait(false))
        {
            Debug.WriteLine($"[SessionSyncService] Login sync already in progress for user {userId}, skipping duplicate trigger.");
            return;
        }

        try
        {
            // Step 1: Claim all orphan sessions (mark UserId + upload)
            await ClaimOrphanSessionsAsync(userId).ConfigureAwait(false);

            // Step 2: Download ALL user sessions from Supabase
            await DownloadUserSessionsAsync(userId).ConfigureAwait(false);

            // Step 3: Retry any previously failed uploads
            await ProcessRetryQueueAsync().ConfigureAwait(false);

            // Notify UI to refresh
            SyncCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Login sync failed: {ex.Message}");
        }
        finally
        {
            _loginSyncLock.Release();
        }
    }

    private async Task ClaimOrphanSessionsAsync(string userId)
    {
        var orphans = await _sessionRepo.GetOrphanSessionsAsync().ConfigureAwait(false);
        Debug.WriteLine($"[SessionSyncService] Claiming {orphans.Count} orphan session(s) for user {userId}");

        foreach (var orphan in orphans)
        {
            try
            {
                // Mark locally with the user ID
                await _sessionRepo.UpdateSessionUserIdAsync(orphan.Id, userId).ConfigureAwait(false);

                // Update the in-memory model for the upload
                orphan.UserId = userId;

                // Retrieve events for this orphan
                var events = await _sessionRepo.GetSessionEventsAsync(orphan.Id).ConfigureAwait(false);

                // Upload to Supabase
                await UploadSessionAsync(orphan, events).ConfigureAwait(false);
                Debug.WriteLine($"[SessionSyncService] Claimed and uploaded orphan session {orphan.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SessionSyncService] Failed to claim orphan session {orphan.Id}: {ex.Message}");
                // Continue with next orphan — one failure should not block others
            }
        }
    }

    private async Task DownloadUserSessionsAsync(string userId)
    {
        try
        {
            var result = await _supabase
                .From<SessionSupabase>()
                .Where(x => x.UserId == userId)
                .Get()
                .ConfigureAwait(false);

            var remoteSessions = result.Models;
            Debug.WriteLine($"[SessionSyncService] Downloading {remoteSessions.Count} remote session(s) for user {userId}");

            foreach (var remote in remoteSessions)
            {
                try
                {
                    // Map SessionSupabase → Session (SQLite model)
                    // Note: the existence check is intentionally removed here.
                    // InsertDownloadedSessionAsync uses InsertOrIgnore atomically,
                    // so a pre-check here would only re-introduce the TOCTOU race.
                    var session = new Session
                    {
                        Id = remote.Id,
                        UserId = remote.UserId,
                        PatientName = remote.PatientName,
                        PatientLastName = remote.PatientLastName,
                        PatientDNI = remote.PatientDNI,
                        SessionStartTime = remote.SessionStartTime,
                        SessionEndTime = remote.SessionEndTime,
                        CreatedAt = remote.CreatedAt
                    };

                    // Query remote events for this session
                    var eventsResult = await _supabase
                        .From<EventSupabase>()
                        .Where(x => x.SessionId == remote.Id)
                        .Get()
                        .ConfigureAwait(false);

                    // Map EventSupabase → EventRecordEntity
                    var eventEntities = eventsResult.Models.Select(e => new EventRecordEntity
                    {
                        Id = e.Id,
                        SessionId = e.SessionId,
                        Timestamp = e.Timestamp,
                        ElapsedTicks = e.ElapsedTicks,
                        EventType = e.EventType,
                        Description = e.Description,
                        Details = e.Details
                    }).ToList();

                    await _sessionRepo.InsertDownloadedSessionAsync(session, eventEntities).ConfigureAwait(false);
                    Debug.WriteLine($"[SessionSyncService] Downloaded session {remote.Id} with {eventEntities.Count} event(s)");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SessionSyncService] Failed to download session {remote.Id}: {ex.Message}");
                    // Continue with next session
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] DownloadUserSessionsAsync failed: {ex.Message}");
        }
    }

    // ============ Internal Upload ============

    private async Task UploadSessionInternalAsync(Session session, List<EventRecord> events)
    {
        // Map Session → SessionSupabase
        var sessionModel = new SessionSupabase
        {
            Id = session.Id,
            UserId = session.UserId,
            PatientName = session.PatientName,
            PatientLastName = session.PatientLastName,
            PatientDNI = session.PatientDNI,
            SessionStartTime = session.SessionStartTime,
            SessionEndTime = session.SessionEndTime,
            CreatedAt = session.CreatedAt
        };

        // Insert session into Supabase
        await _supabase.From<SessionSupabase>().Insert(sessionModel).ConfigureAwait(false);

        // Map List<EventRecord> → List<EventSupabase> and insert in batches of 100
        var eventModels = events.Select(evt => new EventSupabase
        {
            Id = evt.Id,
            SessionId = session.Id,
            Timestamp = evt.Timestamp,
            ElapsedTicks = evt.ElapsedSinceStart.Ticks,
            EventType = evt.EventType,
            Description = evt.Description,
            Details = evt.Details
        }).ToList();

        const int batchSize = 100;
        for (int i = 0; i < eventModels.Count; i += batchSize)
        {
            var batch = eventModels.Skip(i).Take(batchSize).ToList();
            await _supabase.From<EventSupabase>().Insert(batch).ConfigureAwait(false);
        }
    }

    // ============ Retry Queue ============

    private void EnqueueRetry(Func<Task> operation)
    {
        _uploadQueue.Enqueue((operation, 0));
        StartRetryTimerIfNeeded();
    }

    private void StartRetryTimerIfNeeded()
    {
        if (_retryTimer != null && _retryTimer.IsRunning)
            return;

        _retryTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_retryTimer == null)
            return;

        _retryTimer.Interval = TimeSpan.FromSeconds(_retryIntervalSeconds);
        _retryTimer.IsRepeating = false;
        _retryTimer.Tick += async (sender, args) =>
        {
            await ProcessRetryQueueAsync().ConfigureAwait(false);
        };
        _retryTimer.Start();
    }

    private async Task ProcessRetryQueueAsync()
    {
        if (_uploadQueue.Count == 0)
            return;

        Debug.WriteLine($"[SessionSyncService] Processing retry queue ({_uploadQueue.Count} item(s))");

        var itemsToProcess = _uploadQueue.Count;
        var failedItems = new List<(Func<Task> Operation, int AttemptCount)>();

        for (int i = 0; i < itemsToProcess; i++)
        {
            if (_uploadQueue.Count == 0)
                break;

            var item = _uploadQueue.Dequeue();

            if (item.AttemptCount >= MaxRetryAttempts)
            {
                Debug.WriteLine($"[SessionSyncService] Dropping upload after {item.AttemptCount} failed attempts.");
                continue;
            }

            try
            {
                await item.Operation().ConfigureAwait(false);
                Debug.WriteLine($"[SessionSyncService] Retry succeeded on attempt {item.AttemptCount + 1}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SessionSyncService] Retry attempt {item.AttemptCount + 1} failed: {ex.Message}");
                failedItems.Add((item.Operation, item.AttemptCount + 1));
            }
        }

        // Re-enqueue failed items
        foreach (var failed in failedItems)
        {
            _uploadQueue.Enqueue(failed);
        }

        // If there are still items, schedule next retry with exponential backoff
        if (_uploadQueue.Count > 0)
        {
            _retryIntervalSeconds = Math.Min(_retryIntervalSeconds * 2, MaxRetryIntervalSeconds);
            Debug.WriteLine($"[SessionSyncService] Scheduling next retry in {_retryIntervalSeconds}s");
            StartRetryTimerIfNeeded();
        }
        else
        {
            _retryIntervalSeconds = 30; // Reset backoff on full success
        }
    }
}
