# Phase 6: Cloud PostgreSQL Sync (Realtime + Persistent Queue) - Research

**Researched:** 01/04/2026
**Domain:** Supabase Realtime (WebSocket), .NET MAUI Connectivity, Persistent Retry Queue, Toast Notifications
**Confidence:** HIGH

## Summary

Phase 6 extends the existing `SessionSyncService` (built in Phase 05.2) with Supabase Realtime subscriptions to detect sessions inserted by other devices, a persisted retry queue that survives app restarts, connectivity-aware fallback sync, and UI indicators for sync state.

The `supabase-csharp` SDK (v1.1.1, already installed) includes `Supabase.Realtime` as a bundled dependency. The Realtime client provides WebSocket-based `postgres_changes` subscriptions that integrate directly with the existing `SessionSupabase` Postgrest models. The `PostgresChangesOptions` class supports schema, table, event type, and filter parameters — enabling a subscription filtered to `INSERT` events on the `sessions` table for the current user. The SDK handles automatic reconnection after the initial WebSocket connection is established.

For UI notifications, `CommunityToolkit.Maui` (v9.0.0, already installed) provides `Snackbar.Make()` — a timed, non-blocking notification ideal for "N sesiones sincronizadas" messages. Network connectivity monitoring uses `Connectivity.Current.ConnectivityChanged` from `Microsoft.Maui.Networking`, which is built into .NET MAUI. The persisted retry queue maps naturally to a new SQLite table, loaded on startup and processed by the existing exponential backoff timer logic.

**Primary recommendation:** Use `supabase.Realtime.Channel()` + `PostgresChangesOptions` with `user_id=eq.{userId}` filter for INSERT subscriptions. Wrap in `ISessionSyncService` with Start/Stop lifecycle tied to auth state. Persist retry queue in a new `SyncQueue` SQLite table.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **Supcripción en tiempo real de Supabase (postgres_changes)** — Subscribe to `sessions` table for current user. When a session is inserted from any device, all other devices receive the notification in real time.
- **WebSocket nativo de Supabase** — SDK C# already supports Realtime via Postgrest. Use as transport instead of HTTP polling.
- **Monitoreo de conectividad como respaldo** — If WebSocket fails, use connectivity monitoring + periodic sync (e.g. every 30 minutes) as fallback.
- **Cola de re memoria persistida** — Pending sync sessions from 05.2 (in memory) are persisted in SQLite (separate table or field in Session) to survive app restarts.
- **Procesamiento en segundo plano** — Subscription handler runs sync in a background service with `Task.Run`.
- **Ambos dispositivos sincronizan automáticamente** — Both directions auto-sync. Sessions are immutable with unique GUIDs. GUID dedup (already implemented in 05.2).
- **Merge al re-login** — On re-login, the 05.2 flow (claim orphans + download all) is maintained.
- **Notificación sutil tipo toast** — "N sesiones sincronizadas" appears briefly. No notification for locally-created sessions. Auto-dismiss 3-5 seconds.
- **Expandir el ☁️ en 05.2** — Green (synced+realtime), yellow (syncing), gray (offline). Same position as 05.2 indicator.

### Claude's Discretion
- Estructura exacta del handler de suscripción en tiempo real (callback, threading, error handling)
- Intervalo del fallback polling (si WebSocket no disponible)
- Estructura de la cola persistida en SQLite (campo en Session vs tabla separada)
- Formato exacto del toast de notificación
- Colores exactos del indicador ☁️ según estado (amarillo/gris/verde)
- Manejo de reconexión del WebSocket (reconnect automático vs manual)
- Threading del procesamiento (Task.RunInBackground vs hilo)

### Deferred Ideas (OUT OF SCOPE)
- Dashboard web para consultar sesiones en la nube
- Sync selectivo (elegir qué sesiones sincronizar)
- Compartir sesiones con miembros del equipo
- Limitación de sesiones locales por storage
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| SYNC-01 | Sistema soporta sincronización opcional con la nube cuando hay conectividad | Realtime subscription + fallback polling + persisted retry queue enable continuous cloud sync |
</phase_requirements>

## Standard Stack

### Core (already installed)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| **Supabase** (NuGet) | 1.1.1 | Main Supabase client, bundles Realtime | Already installed. Includes `Supabase.Realtime` as dependency. |
| **Supabase.Realtime** | ~7.x (bundled) | WebSocket postgres_changes client | Bundled with Supabase package. Provides `RealtimeClient`, `RealtimeChannel`, `PostgresChangesOptions`. |
| **CommunityToolkit.Maui** | 9.0.0 | Snackbar/Toast notifications | Already installed. Provides `Snackbar.Make()` for non-blocking notifications. |
| **sqlite-net-pcl** | 1.9.172 | Persistent retry queue storage | Already installed. Same `SQLiteAsyncConnection` used by `SessionRepository`. |
| **Microsoft.Maui.Networking** | built-in | `Connectivity.Current` for network state | Built into .NET MAUI, no extra package needed. |

### No New Packages Required
Phase 6 uses **only already-installed packages**. No `dotnet add package` commands needed.

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| CommunityToolkit Snackbar | Custom toast view | Snackbar is cross-platform, timed, zero-setup on Android/iOS. Custom view requires per-platform rendering. |
| SQLite SyncQueue table | Add columns to Session table | Separate table is cleaner: doesn't pollute Session model, supports multiple queue items per session (retries), easier to purge. Recommended. |
| `Connectivity.Current` | `Plugin.Connectivity` (deprecated) | `Connectivity.Current` is built into MAUI. Plugin is deprecated. |

## Architecture Patterns

### Recommended Project Structure
```
AclsTracker/
├── Services/
│   └── Sync/
│       ├── ISessionSyncService.cs       # EXTEND with StartRealtimeSync/StopRealtimeSync
│       ├── SessionSyncService.cs         # EXTEND with realtime subscription handler
│       └── SyncState.cs                  # NEW: enum for sync status (Synced, Syncing, Offline)
├── Models/
│   ├── SyncQueueItem.cs                  # NEW: SQLite model for persisted retry queue
│   ├── SessionSupabase.cs               # EXISTING: reused for Realtime change deserialization
│   └── EventSupabase.cs                 # EXISTING: reused for downloading events
├── ViewModels/
│   └── HistorialViewModel.cs            # EXTEND: listen to sync state changes, show toast
├── Views/
│   └── HistorialPage.xaml               # EXTEND: dynamic color ☁️ indicator
└── Services/
    └── Database/
        ├── ISessionRepository.cs        # EXTEND: SyncQueue CRUD methods
        └── SessionRepository.cs         # EXTEND: SyncQueue table + operations
```

### Pattern 1: Realtime Subscription Lifecycle
**What:** Subscribe to INSERT events on `sessions` table filtered by `user_id`, managed by auth state.
**When to use:** Whenever the user is logged in and the app is in the foreground.
**Example:**
```csharp
// Source: realtime-csharp Client.cs + RealtimeChannel.cs (GitHub)
// In SessionSyncService:

private RealtimeChannel? _sessionChannel;

public async Task StartRealtimeSyncAsync(string userId)
{
    // 1. Connect the Realtime WebSocket (if not already connected)
    if (_supabase.Realtime.Socket == null || !_supabase.Realtime.Socket.IsConnected)
    {
        await _supabase.Realtime.ConnectAsync();
    }

    // 2. Create a channel with postgres_changes options
    var channelName = $"sessions-sync-{userId}";
    _sessionChannel = _supabase.Realtime.Channel(channelName);

    // 3. Register for INSERT events on sessions table, filtered by user_id
    _sessionChannel.Register(new PostgresChangesOptions(
        schema: "public",
        table: "sessions",
        eventType: PostgresChangesOptions.ListenType.Inserts,
        filter: $"user_id=eq.{userId}"
    ));

    // 4. Add the handler
    _sessionChannel.AddPostgresChangeHandler(
        PostgresChangesOptions.ListenType.Inserts,
        OnSessionInserted
    );

    // 5. Subscribe (awaitable - confirms server subscription)
    await _sessionChannel.Subscribe();
}

private async void OnSessionInserted(object sender, PostgresChangesResponse change)
{
    // Deserialize the inserted row into our existing model
    var sessionModel = change.Model<SessionSupabase>();
    if (sessionModel == null) return;

    // Download and insert locally (reuses existing dedup logic)
    await DownloadSingleSessionAsync(sessionModel.Id);
}

public void StopRealtimeSync()
{
    _sessionChannel?.Unsubscribe();
    _sessionChannel = null;
    // Note: Do NOT disconnect the full Realtime client here,
    // as it may be reused. Disconnect on sign-out only.
}
```

### Pattern 2: Auth-Driven Realtime Lifecycle
**What:** Start/stop realtime subscription when auth state changes.
**When to use:** Tied to `IAuthService.AuthStateChanged` event.
**Example:**
```csharp
// In SessionSyncService constructor or init:
_authService.AuthStateChanged += OnAuthStateChangedForRealtime;

private async void OnAuthStateChangedForRealtime(object? sender, bool isLoggedIn)
{
    if (isLoggedIn)
    {
        var userId = _authService.CurrentUserId;
        if (!string.IsNullOrEmpty(userId))
        {
            // Existing 05.2 login sync runs first
            await HandleLoginSyncAsync(userId);

            // Then start realtime subscription
            await StartRealtimeSyncAsync(userId);
        }
    }
    else
    {
        // Stop realtime before cleanup
        StopRealtimeSync();
    }
}
```

**Important:** The `supabase-csharp` Client already auto-calls `Realtime.SetAuth(accessToken)` on `SignedIn`/`TokenRefreshed` events, and auto-unsubscribes all channels on `SignedOut`. This is handled in `Client.Auth_StateChanged()`. Our code needs to explicitly call `ConnectAsync()` and create/subscribe channels, but token passing is automatic.

### Pattern 3: Connectivity Fallback
**What:** When WebSocket is unavailable, fall back to periodic sync using connectivity monitoring.
**When to use:** On `ConnectivityChanged` event when network returns.
**Example:**
```csharp
// In SessionSyncService:
private bool _isRealtimeConnected = false;

// Subscribe to connectivity changes
Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
{
    if (e.NetworkAccess == NetworkAccess.Internet)
    {
        // Network restored — try to reconnect realtime
        try
        {
            var userId = _authService.CurrentUserId;
            if (!string.IsNullOrEmpty(userId) && _sessionChannel == null)
            {
                await StartRealtimeSyncAsync(userId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Realtime reconnect failed: {ex.Message}");
        }

        // Always process retry queue when connectivity returns
        await ProcessRetryQueueAsync();
    }
}
```

### Pattern 4: Persistent Retry Queue (SQLite)
**What:** Store pending upload operations in SQLite so they survive app restarts.
**When to use:** Every failed upload should be persisted immediately.
**Example:**
```csharp
// New model: SyncQueueItem.cs
[Table("SyncQueue")]
public class SyncQueueItem
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>SessionId being synced.</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>JSON-serialized Session data for re-upload.</summary>
    public string SessionData { get; set; } = string.Empty;

    /// <summary>JSON-serialized List<EventSupabase> data for re-upload.</summary>
    public string EventsData { get; set; } = string.Empty;

    /// <summary>Number of retry attempts so far.</summary>
    public int AttemptCount { get; set; }

    /// <summary>When this item was first enqueued.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Next scheduled retry time (for exponential backoff).</summary>
    public DateTime NextRetryAt { get; set; }
}

// In SessionRepository — add SyncQueue table and CRUD:
public async Task<List<SyncQueueItem>> GetPendingSyncItemsAsync()
{
    await EnsureInitializedAsync();
    return await _database.Table<SyncQueueItem>()
        .Where(i => i.NextRetryAt <= DateTime.UtcNow)
        .OrderBy(i => i.CreatedAt)
        .ToListAsync();
}

public async Task EnqueueSyncItemAsync(SyncQueueItem item)
{
    await EnsureInitializedAsync();
    await _database.InsertOrReplaceAsync(item);
}

public async Task RemoveSyncItemAsync(string id)
{
    await EnsureInitializedAsync();
    await _database.DeleteAsync<SyncQueueItem>(id);
}
```

### Pattern 5: Toast Notification
**What:** Show non-blocking notification when sessions arrive from other devices.
**When to use:** After downloading sessions triggered by Realtime INSERT event.
**Example:**
```csharp
// Source: CommunityToolkit.Maui Snackbar docs
using CommunityToolkit.Maui.Alerts;

private async Task ShowSyncToastAsync(int count)
{
    var text = $"{count} sesi{(count == 1 ? "ón" : "ones")} sincronizada{(count == 1 ? "" : "s")}";
    var duration = TimeSpan.FromSeconds(3);
    var snackbar = Snackbar.Make(text, duration: duration);
    await snackbar.Show(CancellationToken.None);
}
```

### Anti-Patterns to Avoid
- **Don't use `supabase.From<SessionSupabase>().On()` shortcut** — This method auto-calls `ConnectAsync()` internally and creates its own channel management. For our lifecycle control (connect on login, disconnect on logout), use the lower-level `supabase.Realtime.Channel()` API directly.
- **Don't store `Func<Task>` in SQLite** — The existing in-memory retry queue stores `Func<Task>` delegates. These cannot be serialized. Persist the serialized session/events data instead and reconstruct the operation on load.
- **Don't subscribe to all events on the table** — Always filter with `user_id=eq.{userId}`. Without the filter, every INSERT on the sessions table would trigger the handler (RLS would still apply, but the filter reduces unnecessary network traffic and processing).
- **Don't call `Realtime.Disconnect()` on every logout if you can just unsubscribe** — Disconnect kills the WebSocket entirely. Prefer `channel.Unsubscribe()` and only `Disconnect()` if the user signs out completely.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| WebSocket reconnection | Custom reconnect logic | Built into `realtime-csharp` v6+ (auto-reconnects after initial connection) | Complex: exponential backoff, heartbeat, state management. Already handled. |
| Auth token passing to Realtime | Manual `SetAuth()` calls | `supabase-csharp` Client auto-passes tokens on `SignedIn`/`TokenRefreshed` | Already handled in `Client.Auth_StateChanged()`. |
| JSON serialization of queue items | Custom binary serialization | `System.Text.Json` or `Newtonsoft.Json` (already used by Supabase SDK) | Consistent with existing serialization throughout the app. |
| Network connectivity detection | Manual ping/HTTP check | `Connectivity.Current.ConnectivityChanged` | Built into MAUI, handles all platform differences. |
| Notification system | Custom overlay/page | `CommunityToolkit.Maui.Alerts.Snackbar` | Cross-platform, timed, zero-config on Android/iOS. |

**Key insight:** The supabase-csharp SDK handles the hardest parts (WebSocket lifecycle, auto-reconnect, token refresh propagation, RLS-aware filtering). The implementation mainly needs to wire these into the existing service architecture.

## Common Pitfalls

### Pitfall 1: Realtime Not Enabled on Table
**What goes wrong:** Subscriptions succeed but no events are received.
**Why it happens:** The Supabase Dashboard has a toggle per-table for Realtime. By default, tables are NOT in the `supabase_realtime` publication.
**How to avoid:** Run `ALTER PUBLICATION supabase_realtime ADD TABLE sessions;` (and `events` table if needed). Verify in Dashboard → Database → Publications.
**Warning signs:** `Subscribe()` returns successfully, but handler never fires.

### Pitfall 2: RLS Blocks Realtime Events
**What goes wrong:** Events are not delivered even though the subscription is active and the table is published.
**Why it happens:** Realtime uses RLS policies to determine which clients can receive events. The current RLS policy (`user_id = auth.uid()`) is correct, but it requires the JWT to be set on the Realtime client.
**How to avoid:** Ensure `supabase.Realtime.SetAuth(accessToken)` is called after login. The `supabase-csharp` Client does this automatically in `Auth_StateChanged`, but ONLY if the `AuthStateChanged` event fires. Verify that the auth session is active before subscribing.
**Warning signs:** Local inserts trigger the handler (same device), but remote inserts do not.

### Pitfall 3: Channel Name Collision
**What goes wrong:** Second subscription fails silently or replaces the first.
**Why it happens:** Channel names must be unique. If you use a static name like `"sessions"`, two calls to `Channel()` return the same channel object (cached by the SDK).
**How to avoid:** Include the `userId` in the channel name: `$"sessions-sync-{userId}"`. The SDK caches channels by topic, so this ensures uniqueness per user.
**Warning signs:** Only one subscription active after multiple login/logout cycles.

### Pitfall 4: Race Condition on Download
**What goes wrong:** Session downloaded via Realtime but events query fails because events aren't committed yet.
**Why it happens:** Supabase Realtime fires the event when the row appears in the replication slot. If events are inserted in a separate batch transaction (as our upload does), there may be a brief window where the session exists but events don't.
**How to avoid:** Add a small delay (e.g., 500ms) before downloading events, or retry the events query if it returns empty. Alternatively, subscribe to events table as well. The simpler approach is to just retry the events download if empty (sessions without events are valid — the user may still be recording).
**Warning signs:** Downloaded sessions show 0 events intermittently.

### Pitfall 5: Snackbar on Background Thread
**What goes wrong:** `Snackbar.Show()` throws `InvalidOperationException` or doesn't appear.
**Why it happens:** UI operations must run on the main thread. The Realtime handler runs on a background thread.
**How to avoid:** Wrap the Snackbar call in `MainThread.InvokeOnMainThreadAsync()`.
**Warning signs:** Toast doesn't appear or app crashes silently.

### Pitfall 6: Memory Leak from Event Handlers
**What goes wrong:** After login/logout cycles, old event handlers pile up, causing duplicate notifications.
**Why it happens:** `Connectivity.Current.ConnectivityChanged` is a static event. If you subscribe in the constructor without unsubscribing, each service instance adds a new handler.
**How to avoid:** Unsubscribe in `StopRealtimeSync()` or implement `IDisposable`. Use a flag to prevent double-subscription.
**Warning signs:** Same notification appears 2, 3, 4... times after multiple logins.

### Pitfall 7: Windows Snackbar Requires Extra Setup
**What goes wrong:** Snackbar doesn't work on Windows.
**Why it happens:** CommunityToolkit Snackbar on Windows requires `options.SetShouldEnableSnackbarOnWindows(true)` in `UseMauiCommunityToolkit()` and Package.appxmanifest changes.
**How to avoid:** If Windows support is needed, add the Windows snackbar setup. Since the project primarily targets Android/iOS, this is low risk but should be noted.
**Warning signs:** Snackbar works on Android/iOS but not Windows.

## Code Examples

### Complete Realtime Subscription Setup
```csharp
// Source: realtime-csharp README + Client.cs source (GitHub)
// Pattern for subscribing to INSERT on sessions filtered by user_id

public async Task StartRealtimeSyncAsync(string userId)
{
    try
    {
        // Connect WebSocket if not already
        if (_supabase.Realtime.Socket == null || !_supabase.Realtime.Socket.IsConnected)
        {
            await _supabase.Realtime.ConnectAsync();
        }

        // Create channel (unique per user)
        var channelName = $"sessions-sync-{userId}";
        _sessionChannel = _supabase.Realtime.Channel(channelName);

        // Register postgres_changes: INSERT on public.sessions WHERE user_id = {userId}
        _sessionChannel.Register(new PostgresChangesOptions(
            schema: "public",
            table: "sessions",
            eventType: PostgresChangesOptions.ListenType.Inserts,
            filter: $"user_id=eq.{userId}"
        ));

        // Add handler
        _sessionChannel.AddPostgresChangeHandler(
            PostgresChangesOptions.ListenType.Inserts,
            async (sender, change) =>
            {
                var session = change.Model<SessionSupabase>();
                if (session == null) return;

                // Skip if already exists locally (dedup)
                var existing = await _sessionRepo.GetSessionAsync(session.Id);
                if (existing != null) return;

                // Download session + events
                await DownloadSingleSessionAsync(session);
            }
        );

        // Monitor channel state
        _sessionChannel.AddStateChangedHandler((sender, state) =>
        {
            Debug.WriteLine($"[Realtime] Channel state: {state}");
            UpdateSyncState(state == ChannelState.Joined
                ? SyncState.Synced
                : SyncState.Syncing);
        });

        // Subscribe (awaitable)
        await _sessionChannel.Subscribe();

        Debug.WriteLine($"[SessionSyncService] Realtime subscription active for user {userId}");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[SessionSyncService] Realtime start failed: {ex.Message}");
        // Fallback: rely on periodic sync / connectivity monitoring
    }
}
```

### Downloading a Single Session (from Realtime trigger)
```csharp
private async Task DownloadSingleSessionAsync(SessionSupabase remote)
{
    try
    {
        // Dedup check
        var existing = await _sessionRepo.GetSessionAsync(remote.Id);
        if (existing != null) return;

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
        var eventsResult = await _supabase
            .From<EventSupabase>()
            .Where(x => x.SessionId == remote.Id)
            .Get();

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

        await _sessionRepo.InsertDownloadedSessionAsync(session, eventEntities);

        // Show toast on main thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var text = "1 sesión sincronizada";
            var snackbar = Snackbar.Make(text, duration: TimeSpan.FromSeconds(3));
            await snackbar.Show(CancellationToken.None);
        });

        // Notify UI to refresh
        SyncCompleted?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[SessionSyncService] Download failed for session {remote.Id}: {ex.Message}");
    }
}
```

### Connectivity Monitoring
```csharp
// Source: Microsoft.Maui.Networking (built into MAUI)
using Microsoft.Maui.Networking;

// Check current state
var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

// Subscribe to changes
Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;

void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
{
    var isNowOnline = e.NetworkAccess == NetworkAccess.Internet;
    Debug.WriteLine($"[Sync] Connectivity changed: {e.NetworkAccess}");

    if (isNowOnline)
    {
        // Process retry queue
        // Attempt realtime reconnect if needed
    }
}
```

### Persistent Retry Queue — Load on Startup
```csharp
// In SessionSyncService initialization or HandleLoginSyncAsync:
private async Task LoadPersistedQueueAsync()
{
    var pendingItems = await _sessionRepo.GetPendingSyncItemsAsync();
    Debug.WriteLine($"[SessionSyncService] Loaded {pendingItems.Count} pending sync item(s)");

    foreach (var item in pendingItems)
    {
        try
        {
            var session = JsonConvert.DeserializeObject<SessionSupabase>(item.SessionData);
            var events = JsonConvert.DeserializeObject<List<EventSupabase>>(item.EventsData);

            if (session == null || events == null) continue;

            await UploadSessionInternalAsync(
                MapToLocalSession(session),
                MapToEventRecords(events)
            );

            await _sessionRepo.RemoveSyncItemAsync(item.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SessionSyncService] Retry failed for item {item.Id}: {ex.Message}");
            // Update attempt count and next retry time
            item.AttemptCount++;
            item.NextRetryAt = DateTime.UtcNow.AddSeconds(
                Math.Min(30 * Math.Pow(2, item.AttemptCount), 300));
            await _sessionRepo.EnqueueSyncItemAsync(item);
        }
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Connect()` sync method | `ConnectAsync()` async | realtime-csharp v6.0.0 | Must use async version; sync version marked `[Obsolete]` |
| `EventHandler<EventArgs>` pattern | Delegates with `Add/Remove/Clear` prefix | realtime-csharp v6.0.0 | Use `AddPostgresChangeHandler` instead of `+=` event syntax |
| Custom reconnect logic | Built-in reconnect via websocket-client | realtime-csharp v6.0.0 | No need to implement reconnect — SDK handles it |
| `realtime-csharp` NuGet name | `Supabase.Realtime` NuGet name | realtime-csharp v7.0.0 | Package renamed; bundled via `Supabase` metapackage |
| `supabase-csharp` NuGet name | `Supabase` NuGet name | supabase-csharp v1.0.0 | Already using the new package name |

**Deprecated/outdated:**
- `Connect()` (sync): Marked `[Obsolete]` in realtime-csharp v6. Use `ConnectAsync()`.
- `realtime-csharp` NuGet package: Renamed to `Supabase.Realtime`. Old package has deprecation notice.

## Open Questions

1. **Events table Realtime subscription needed?**
   - What we know: Sessions are subscribed via Realtime INSERT. Events are downloaded separately via Postgrest query after session is detected.
   - What's unclear: Whether we should also subscribe to the `events` table for real-time updates during an ongoing code.
   - Recommendation: **No.** Events are always uploaded as a batch when the session ends (in `StopCode()`). There's no use case for real-time event streaming. The session INSERT is the trigger that matters.

2. **Batch notification aggregation?**
   - What we know: Multiple sessions could arrive in rapid succession (e.g., after reconnect).
   - What's unclear: Whether to aggregate notifications (e.g., "3 sesiones sincronizadas" vs 3 individual toasts).
   - Recommendation: Aggregate. Collect download count over a 2-second window, then show one toast. This avoids toast spam.

3. **Fallback polling interval?**
   - What we know: CONTEXT says "every 30 minutes" as example. This is Claude's discretion.
   - Recommendation: 5 minutes when realtime is down but network is available. 30 minutes is too long for a medical app where sync timeliness matters.

4. **Snackbar on Windows target?**
   - What we know: The project targets Windows (`net9.0-windows10.0.19041.0`). Snackbar requires extra `Package.appxmanifest` setup on Windows.
   - Recommendation: Add the Windows snackbar setup if the Windows target is actively used. If it's just for development/testing, document the limitation but don't block on it.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit (recommended) or NUnit — verify existing |
| Config file | None detected — verify or create |
| Quick run command | `dotnet test --filter "FullyQualifiedName~SessionSyncService"` |
| Full suite command | `dotnet test` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SYNC-01 | Realtime subscription detects INSERT from other device | unit (mock) | `dotnet test --filter "RealtimeSubscription"` | ❌ Wave 0 |
| SYNC-01 | Downloaded session deduped correctly | unit | `dotnet test --filter "SessionDedup"` | ❌ Wave 0 |
| SYNC-01 | Persistent queue survives restart | unit | `dotnet test --filter "PersistentQueue"` | ❌ Wave 0 |
| SYNC-01 | Toast shown on remote sync | manual-only | N/A — UI verification | N/A |
| SYNC-01 | Sync indicator shows correct state | manual-only | N/A — UI verification | N/A |
| SYNC-01 | Connectivity fallback triggers sync | unit (mock) | `dotnet test --filter "ConnectivityFallback"` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test --filter "FullyQualifiedName~Sync"`
- **Per wave merge:** `dotnet test`
- **Phase gate:** Full suite green before `/gsd-verify-work`

### Wave 0 Gaps
- [ ] `tests/SessionSyncServiceTests.cs` — covers realtime subscription, dedup, queue persistence
- [ ] `tests/SessionRepositoryTests.cs` — covers SyncQueue CRUD
- [ ] Framework install: `dotnet add package xunit` etc. — verify if test infrastructure exists

## Sources

### Primary (HIGH confidence)
- [realtime-csharp GitHub] - Client.cs, RealtimeChannel.cs, PostgresChangesOptions.cs, PostgresChangesResponse.cs source code — direct API verification
- [supabase-csharp GitHub] - Client.cs source code — Auth_StateChanged auto-set-auth, SupabaseTable.On shortcut
- [CommunityToolkit.Maui Snackbar docs] (learn.microsoft.com) — Snackbar.Make() API, platform requirements
- [Supabase Postgres Changes docs] (supabase.com/docs/guides/realtime/postgres-changes) — RLS requirements, filter syntax, publication setup

### Secondary (MEDIUM confidence)
- [realtime-csharp README] - Breaking changes from v5→v6, delegate-based handlers, auto-reconnect
- [supabase-csharp Wiki] - Using the Client, shortcut syntax, Desktop Clients guidance

### Tertiary (LOW confidence)
- None — all findings verified against source code

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages already installed, API verified against source code
- Architecture: HIGH — patterns directly from realtime-csharp source and existing codebase
- Pitfalls: HIGH — documented from SDK source, Supabase official docs, and community issues
- Connectivity fallback: MEDIUM — `Connectivity.Current` is well-documented MAUI API, but behavior on specific Android/iOS versions may vary
- Persistent retry queue: HIGH — standard SQLite pattern, consistent with existing SessionRepository

**Research date:** 01/04/2026
**Valid until:** 01/05/2026 (stable APIs, unlikely to change)
