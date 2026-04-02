---
phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry
plan: 02
subsystem: sync
tags: [realtime, websocket, persistent-queue, connectivity, supabase, retry, sync]

# Dependency graph
requires:
  - phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry
    provides: SyncQueueItem model, SyncState enum, extended ISessionSyncService and ISessionRepository
provides:
  - Full realtime sync engine in SessionSyncService with Supabase Realtime WebSocket subscriptions
  - Persistent retry queue using SQLite SyncQueue table (survives app restart)
  - Connectivity-aware fallback that reconnects realtime and processes queue on network restore
  - SyncState lifecycle: Offline to Syncing to Synced with event-driven UI notifications
affects: [06-03]

# Tech tracking
tech-stack:
  added: []
  patterns: [realtime-websocket-subscription, persistent-retry-queue, connectivity-fallback, auth-driven-lifecycle]

key-files:
  created: []
  modified:
    - AclsTracker/Services/Sync/SessionSyncService.cs

key-decisions:
  - "Used RealtimeChannel concrete type (not IRealtimeChannel interface) because Register/AddPostgresChangeHandler/Subscribe methods are on concrete class only"
  - "Replaced in-memory Queue of Func<Task> with SQLite SyncQueue table for persistence across app restarts"
  - "500ms delay in DownloadSingleSessionAsync to avoid events race condition (RESEARCH Pitfall 4)"
  - "Connectivity fallback subscribes on realtime failure, unsubscribes on stop to prevent memory leak (RESEARCH Pitfall 6)"

patterns-established:
  - "Persistent retry queue: serialize session/events to JSON, store in SyncQueue SQLite table, deserialize on retry"
  - "Realtime lifecycle: start on login after claim+download+queue, stop on logout before cleanup"
  - "Connectivity fallback: subscribe to ConnectivityChanged when realtime fails, unsubscribe when realtime succeeds or on logout"

requirements-completed: [SYNC-01]

# Metrics
duration: 85min
completed: 2026-04-02
---

# Phase 06 Plan 02: SessionSyncService Realtime Engine Summary

**Full Supabase Realtime WebSocket sync engine with persistent SQLite retry queue, connectivity-aware fallback, and auth-driven lifecycle replacing in-memory retry**

## Performance

- **Duration:** 85 min
- **Started:** 2026-04-02T00:47:30Z
- **Completed:** 2026-04-02T02:13:15Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- Rewrote SessionSyncService from in-memory retry queue to full persistent SQLite-based retry queue that survives app restarts
- Implemented Supabase Realtime WebSocket subscription for INSERT events on sessions table filtered by user_id
- Added realtime-triggered single-session download with GUID dedup and 500ms delay for events race condition
- Implemented connectivity-aware fallback that reconnects realtime and processes retry queue when network returns
- Added SyncState lifecycle management (Offline, Syncing, Synced) with SyncStateChanged event for UI binding
- Auth-driven lifecycle: realtime starts on login (after claim+download+queue), stops on logout (before cleanup)

## Task Commits

Each task was committed atomically:

1. **Task 1: Rewrite SessionSyncService with Realtime subscription, persistent queue, and connectivity fallback** - `78a12e9` (feat)

## Files Created/Modified
- `AclsTracker/Services/Sync/SessionSyncService.cs` - Complete rewrite: removed in-memory queue, added realtime sync, persistent retry queue, connectivity fallback, sync state management

## Decisions Made
- Used `RealtimeChannel` concrete type (not `IRealtimeChannel` interface) because `Register`, `AddPostgresChangeHandler`, `Subscribe`, and `Unsubscribe` methods are only on the concrete class
- Replaced `Queue<(Func<Task>, int)>` with `SyncQueueItem` SQLite persistence — failed uploads now survive app restart
- Added 500ms delay before downloading events in realtime handler to prevent race condition where events are not committed yet
- Connectivity fallback uses subscribe/unsubscribe pattern to prevent memory leak from static `ConnectivityChanged` event

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Changed IRealtimeChannel to RealtimeChannel concrete type**
- **Found during:** Task 1 (SessionSyncService rewrite)
- **Issue:** Plan specified `IRealtimeChannel?` but `Register`, `AddPostgresChangeHandler`, `Subscribe`, `Unsubscribe` methods are not on the interface — only on the concrete `RealtimeChannel` class
- **Fix:** Changed field type from `IRealtimeChannel?` to `RealtimeChannel?` in `Supabase.Realtime` namespace
- **Files modified:** AclsTracker/Services/Sync/SessionSyncService.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 78a12e9 (Task 1 commit)

**2. [Rule 3 - Blocking] Removed unused Supabase.Realtime.Channel using directive**
- **Found during:** Task 1 (build verification)
- **Issue:** `using Supabase.Realtime.Channel;` namespace is for internal types like `ChannelOptions` and `Push`, not needed for our API usage
- **Fix:** Removed the unused using directive
- **Files modified:** AclsTracker/Services/Sync/SessionSyncService.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 78a12e9 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both were type resolution fixes — the plan's logic was correct, just the exact type names needed adjustment based on actual SDK API surface.

## Issues Encountered
- Build target framework is `net9.0-android36.0` (not `net9.0-android`) — adjusted build command accordingly (pre-existing, noted in 06-01 SUMMARY)

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- SessionSyncService fully implements ISessionSyncService with all new members
- Realtime sync engine ready for UI integration in Plan 03 (sync indicator, toast notifications)
- All existing consumers (MainViewModel, AuthViewModel, HistorialViewModel) still compile
- Build succeeds with zero errors

---
*Phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry*
*Completed: 2026-04-02*
