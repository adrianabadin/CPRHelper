using System.Diagnostics;
using System.Text.Json;
using AclsTracker.Models;
using AclsTracker.Services.Auth;
using AclsTracker.Services.Database;
using Microsoft.Maui.Networking;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;

namespace AclsTracker.Services.Sync;

/// <summary>
/// Core orchestration layer that coordinates upload, download, claim, cleanup,
/// realtime subscriptions, persistent retry queue, and connectivity-aware fallback
/// between local SQLite and Supabase.
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

    // Retry constants
    private const int MaxRetryAttempts = 5;
    private const int MaxRetryIntervalSeconds = 300; // 5 minutes

    // Realtime subscription
    private RealtimeChannel? _sessionChannel;
    private SyncState _currentSyncState = SyncState.Offline;
    private bool _isConnectivitySubscribed = false;
    private string? _fallbackUserId;

    public event EventHandler? SyncCompleted;
    public event EventHandler<SyncState>? SyncStateChanged;
    public event EventHandler<int>? SessionsDownloaded;

    public SyncState CurrentSyncState => _currentSyncState;

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
            Debug.WriteLine($"[SessionSyncService] Upload failed for session {session.Id}: {ex.Message}. Persisting to retry queue.");
            await PersistToQueueAsync(session, events).ConfigureAwait(false);
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

    // ============ Realtime Sync ============

    public async Task StartRealtimeSyncAsync(string userId)
    {
        try
        {
            // Don't double-subscribe
            if (_sessionChannel != null) return;

            // Connect WebSocket if not already connected
            if (_supabase.Realtime.Socket == null || !_supabase.Realtime.Socket.IsConnected)
            {
                await _supabase.Realtime.ConnectAsync().ConfigureAwait(false);
            }

            // Create unique channel per user (prevents collision - see RESEARCH Pitfall 3)
            var channelName = $"sessions-sync-{userId}";
            _sessionChannel = _supabase.Realtime.Channel(channelName);

            // Register for INSERT events filtered by user_id
            _sessionChannel.Register(new PostgresChangesOptions(
                schema: "public",
                table: "sessions",
                eventType: PostgresChangesOptions.ListenType.Inserts,
                filter: $"user_id=eq.{userId}"
            ));

            // Add handler for INSERT events from other devices
            _sessionChannel.AddPostgresChangeHandler(
                PostgresChangesOptions.ListenType.Inserts,
                OnRemoteSessionInserted
            );

            await _sessionChannel.Subscribe().ConfigureAwait(false);

            UpdateSyncState(SyncState.Synced);
            Debug.WriteLine($"[SessionSyncService] Realtime subscription active for user {userId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Realtime start failed: {ex.Message}. Will rely on connectivity fallback.");
            UpdateSyncState(SyncState.Offline);

            // Subscribe to connectivity changes for fallback
            SubscribeToConnectivityChanges(userId);
        }
    }

    public void StopRealtimeSync()
    {
        try
        {
            _sessionChannel?.Unsubscribe();
            _sessionChannel = null;
            UnsubscribeFromConnectivityChanges();
            UpdateSyncState(SyncState.Offline);
            Debug.WriteLine("[SessionSyncService] Realtime subscription stopped");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Error stopping realtime: {ex.Message}");
        }
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
        else
        {
            // Stop realtime subscription before cleanup
            StopRealtimeSync();
        }
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

            // Step 3: Retry any previously failed uploads from persistent queue
            await ProcessPersistedQueueAsync().ConfigureAwait(false);

            // Notify UI to refresh
            SyncCompleted?.Invoke(this, EventArgs.Empty);

            // Step 4: Start realtime subscription for ongoing sync
            await StartRealtimeSyncAsync(userId).ConfigureAwait(false);
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
        var sessionModel = MapSessionToSupabase(session);

        // Insert session into Supabase
        await _supabase.From<SessionSupabase>().Insert(sessionModel).ConfigureAwait(false);

        // Map List<EventRecord> → List<EventSupabase> and insert in batches of 100
        var eventModels = events.Select(evt => MapEventToSupabase(evt, session.Id)).ToList();

        const int batchSize = 100;
        for (int i = 0; i < eventModels.Count; i += batchSize)
        {
            var batch = eventModels.Skip(i).Take(batchSize).ToList();
            await _supabase.From<EventSupabase>().Insert(batch).ConfigureAwait(false);
        }
    }

    // ============ Mapping Helpers ============

    private static SessionSupabase MapSessionToSupabase(Session session)
    {
        return new SessionSupabase
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
    }

    private static EventSupabase MapEventToSupabase(EventRecord evt, string sessionId)
    {
        return new EventSupabase
        {
            Id = evt.Id,
            SessionId = sessionId,
            Timestamp = evt.Timestamp,
            ElapsedTicks = evt.ElapsedSinceStart.Ticks,
            EventType = evt.EventType,
            Description = evt.Description,
            Details = evt.Details
        };
    }

    // ============ Persistent Retry Queue ============

    private async Task PersistToQueueAsync(Session session, List<EventRecord> events)
    {
        var sessionModel = MapSessionToSupabase(session);
        var eventModels = events.Select(evt => MapEventToSupabase(evt, session.Id)).ToList();

        var item = new SyncQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = session.Id,
            SessionData = JsonSerializer.Serialize(sessionModel),
            EventsData = JsonSerializer.Serialize(eventModels),
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            NextRetryAt = DateTime.UtcNow.AddSeconds(30) // first retry in 30s
        };

        await _sessionRepo.EnqueueSyncItemAsync(item).ConfigureAwait(false);
        Debug.WriteLine($"[SessionSyncService] Persisted upload for session {session.Id} to retry queue");
    }

    private async Task ProcessPersistedQueueAsync()
    {
        var pending = await _sessionRepo.GetPendingSyncItemsAsync().ConfigureAwait(false);
        if (pending.Count == 0) return;

        UpdateSyncState(SyncState.Syncing);
        Debug.WriteLine($"[SessionSyncService] Processing {pending.Count} persisted queue item(s)");

        foreach (var item in pending)
        {
            if (item.AttemptCount >= MaxRetryAttempts)
            {
                await _sessionRepo.RemoveSyncItemAsync(item.Id).ConfigureAwait(false);
                Debug.WriteLine($"[SessionSyncService] Dropping queue item after {item.AttemptCount} attempts");
                continue;
            }

            try
            {
                var sessionModel = JsonSerializer.Deserialize<SessionSupabase>(item.SessionData);
                var eventModels = JsonSerializer.Deserialize<List<EventSupabase>>(item.EventsData);
                if (sessionModel == null || eventModels == null) continue;

                // Re-upload session
                await _supabase.From<SessionSupabase>().Insert(sessionModel).ConfigureAwait(false);

                // Re-upload events in batches
                const int batchSize = 100;
                for (int i = 0; i < eventModels.Count; i += batchSize)
                {
                    var batch = eventModels.Skip(i).Take(batchSize).ToList();
                    await _supabase.From<EventSupabase>().Insert(batch).ConfigureAwait(false);
                }

                await _sessionRepo.RemoveSyncItemAsync(item.Id).ConfigureAwait(false);
                Debug.WriteLine($"[SessionSyncService] Queue retry succeeded for {item.SessionId}");
            }
            catch (Exception ex)
            {
                item.AttemptCount++;
                var delay = Math.Min(30 * Math.Pow(2, item.AttemptCount), MaxRetryIntervalSeconds);
                item.NextRetryAt = DateTime.UtcNow.AddSeconds(delay);
                await _sessionRepo.EnqueueSyncItemAsync(item).ConfigureAwait(false);
                Debug.WriteLine($"[SessionSyncService] Queue retry failed (attempt {item.AttemptCount}): {ex.Message}");
            }
        }

        UpdateSyncState(SyncState.Synced);
        SyncCompleted?.Invoke(this, EventArgs.Empty);
    }

    // ============ Realtime Handlers ============

    private async void OnRemoteSessionInserted(object sender, PostgresChangesResponse change)
    {
        try
        {
            var sessionModel = change.Model<SessionSupabase>();
            if (sessionModel == null) return;

            // Download single session (reuses existing dedup + insert logic)
            await DownloadSingleSessionAsync(sessionModel).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Realtime handler error: {ex.Message}");
        }
    }

    private async Task DownloadSingleSessionAsync(SessionSupabase remote)
    {
        // Dedup check
        var existing = await _sessionRepo.GetSessionAsync(remote.Id).ConfigureAwait(false);
        if (existing != null)
        {
            Debug.WriteLine($"[SessionSyncService] Session {remote.Id} already exists locally, skipping realtime download.");
            return;
        }

        // Map to local model
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

        // Download events for this session
        // Add small delay to avoid race condition (RESEARCH Pitfall 4: events may not be committed yet)
        await Task.Delay(500).ConfigureAwait(false);

        var eventsResult = await _supabase
            .From<EventSupabase>()
            .Where(x => x.SessionId == remote.Id)
            .Get()
            .ConfigureAwait(false);

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

        // Notify UI
        SyncCompleted?.Invoke(this, EventArgs.Empty);
        SessionsDownloaded?.Invoke(this, 1);

        Debug.WriteLine($"[SessionSyncService] Downloaded realtime session {remote.Id} with {eventEntities.Count} event(s)");
    }

    // ============ Connectivity Fallback ============

    private void SubscribeToConnectivityChanges(string userId)
    {
        if (_isConnectivitySubscribed) return;
        _isConnectivitySubscribed = true;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        // Store userId for reconnect
        _fallbackUserId = userId;
    }

    private void UnsubscribeFromConnectivityChanges()
    {
        if (!_isConnectivitySubscribed) return;
        _isConnectivitySubscribed = false;
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        _fallbackUserId = null;
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet) return;

        Debug.WriteLine("[SessionSyncService] Connectivity restored");

        var userId = _fallbackUserId ?? _authService.CurrentUserId;
        if (string.IsNullOrEmpty(userId)) return;

        // Try to re-establish realtime if not connected
        if (_sessionChannel == null)
        {
            try
            {
                await StartRealtimeSyncAsync(userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SessionSyncService] Realtime reconnect failed: {ex.Message}");
            }
        }

        // Always process retry queue when connectivity returns
        try
        {
            await ProcessPersistedQueueAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Queue processing after reconnect failed: {ex.Message}");
        }
    }

    // ============ Sync State ============

    private void UpdateSyncState(SyncState newState)
    {
        if (_currentSyncState == newState) return;
        _currentSyncState = newState;
        SyncStateChanged?.Invoke(this, newState);
        Debug.WriteLine($"[SessionSyncService] Sync state changed to {newState}");
    }
}
