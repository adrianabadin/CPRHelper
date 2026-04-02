---
phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry
plan: 03
subsystem: ui-sync
tags: [realtime, sync-indicator, toast, snackbar, datatrigger, mvvm, logout-lifecycle]

# Dependency graph
requires:
  - phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry
    provides: Full SessionSyncService with SyncStateChanged, SessionsDownloaded events and SyncState enum
  - phase: 05.2-vincular-sesiones-de-supabase-al-usuario-logueado
    provides: Per-session cloud indicator in HistorialPage, AuthViewModel logout cleanup
provides:
  - Multi-color sync state indicator in HistorialPage (green/yellow/gray cloud)
  - Toast notification via Snackbar when sessions downloaded from another device
  - Proper realtime lifecycle: StopRealtimeSync before logout cleanup
affects: []

# Tech tracking
tech-stack:
  added: [CommunityToolkit.Maui.Alerts.Snackbar]
  patterns: [sync-state-datatrigger, mainthread-toast-notification, realtime-lifecycle-logout]

key-files:
  created: []
  modified:
    - AclsTracker/ViewModels/HistorialViewModel.cs
    - AclsTracker/Views/HistorialPage.xaml
    - AclsTracker/ViewModels/AuthViewModel.cs

key-decisions:
  - "Global sync state indicator placed next to toggle buttons, per-session cloud stays green for backed-up sessions"
  - "Toast uses CommunityToolkit.Maui.Alerts.Snackbar with 3-second duration on MainThread (not Shell.DisplayAlert)"
  - "StopRealtimeSync called as first step in logout before capturing userId"

patterns-established:
  - "Sync state pattern: ObservableProperty SyncState + computed SyncStateDisplay + XAML DataTriggers for color"
  - "Toast notification pattern: SessionsDownloaded event + MainThread.BeginInvokeOnMainThread + Snackbar.Make"
  - "Logout lifecycle: StopRealtimeSync → capture userId → DeleteLocalUserSessions → SignOut"

requirements-completed: [SYNC-01]

# Metrics
duration: 4min
completed: 2026-04-02
---

# Phase 06 Plan 03: Sync UI Indicator and Realtime Lifecycle Summary

**Multi-color cloud sync indicator with DataTriggers, Snackbar toast for remote session downloads, and realtime WebSocket cleanup on logout**

## Performance

- **Duration:** 4 min
- **Started:** 2026-04-02T15:12:53Z
- **Completed:** 2026-04-02T15:17:10Z
- **Tasks:** 3 (auto), 1 checkpoint pending
- **Files modified:** 3

## Accomplishments
- Added sync state tracking to HistorialViewModel with CurrentSyncState property and SyncStateDisplay computed property
- Implemented global sync state indicator in HistorialPage with green/yellow/gray DataTriggers
- Added Snackbar toast notification for remote session downloads with Spanish pluralization
- Wired AuthViewModel to stop realtime WebSocket before logout cleanup

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend HistorialViewModel with sync state tracking and toast notifications** - `022af4f` (feat)
2. **Task 2: Update HistorialPage.xaml cloud indicator with multi-color DataTriggers** - `5b0a2a3` (feat)
3. **Task 3: Wire AuthViewModel to stop realtime before logout cleanup** - `bbba501` (feat)

## Files Created/Modified
- `AclsTracker/ViewModels/HistorialViewModel.cs` - Added CurrentSyncState, SyncStateDisplay, SyncStateChanged/SessionsDownloaded subscriptions
- `AclsTracker/Views/HistorialPage.xaml` - Added global sync indicator with 3 DataTriggers, updated per-session cloud to green
- `AclsTracker/ViewModels/AuthViewModel.cs` - Added StopRealtimeSync call as first step in SignOutAsync

## Decisions Made
- Global sync state indicator placed in toggle bar area (not per-session) to show connection state vs per-session backup state
- Toast uses Snackbar.Make (not Shell.DisplayAlert) because it auto-dismisses after 3 seconds and doesn't block the user
- StopRealtimeSync is synchronous and called first because it unsubscribes the WebSocket before any cleanup

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase 06 UI integration complete — all sync features wired to UI
- Multi-color indicator shows real-time sync state (green=Synced, yellow=Syncing, gray=Offline)
- Toast notifications appear when sessions arrive from other devices
- Realtime subscription stops cleanly on logout (no crash, no leaked handlers)
- **Checkpoint Task 4 pending** — human verification of realtime sync on device

## Self-Check: PASSED

All files verified present on disk. All 3 task commits confirmed in git log.

---
*Phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry*
*Completed: 2026-04-02*
