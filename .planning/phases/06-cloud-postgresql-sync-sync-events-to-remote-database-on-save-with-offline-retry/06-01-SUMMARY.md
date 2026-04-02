---
phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry
plan: 01
subsystem: sync
tags: [sqlite, sync-queue, retry, realtime, supabase, interfaces]

# Dependency graph
requires:
  - phase: 05.2-vincular-sesiones-de-supabase-al-usuario-logueado
    provides: SessionSyncService with upload/download/retry, ISessionSyncService, ISessionRepository
provides:
  - SyncQueueItem SQLite model for persistent retry queue
  - SyncState enum for UI sync indicator
  - Extended ISessionRepository with SyncQueue CRUD methods
  - Extended ISessionSyncService with realtime lifecycle and sync state
affects: [06-02, 06-03]

# Tech tracking
tech-stack:
  added: []
  patterns: [persistent-retry-queue, sync-state-enum, realtime-lifecycle-interface]

key-files:
  created:
    - AclsTracker/Models/SyncQueueItem.cs
    - AclsTracker/Services/Sync/SyncState.cs
  modified:
    - AclsTracker/Services/Database/ISessionRepository.cs
    - AclsTracker/Services/Database/SessionRepository.cs
    - AclsTracker/Services/Sync/ISessionSyncService.cs
    - AclsTracker/Services/Sync/SessionSyncService.cs

key-decisions:
  - "GetPendingSyncItemsAsync uses raw SQL QueryAsync for sqlite-net-pcl OrderBy compatibility (same pattern as GetSessionsByUserIdAsync)"
  - "SessionSyncService stubs throw NotImplementedException for Plan 02 to implement"
  - "SyncQueueItem is a plain POCO (no base class) following EventRecordEntity pattern for sqlite-net-pcl compatibility"

patterns-established:
  - "SyncQueue section separator pattern: // ============ SyncQueue Operations ============"
  - "Interface extension pattern: add section separators and XML docs for new method groups"

requirements-completed: [SYNC-01]

# Metrics
duration: 9min
completed: 2026-04-02
---

# Phase 06 Plan 01: Data Layer Contracts Summary

**SyncQueueItem SQLite model, SyncState enum, and extended sync/repository interfaces for persistent retry queue and realtime lifecycle**

## Performance

- **Duration:** 9 min
- **Started:** 2026-04-02T00:26:50Z
- **Completed:** 2026-04-02T00:36:13Z
- **Tasks:** 3
- **Files modified:** 6

## Accomplishments
- Created SyncQueueItem model with all retry queue fields (Id, SessionId, SessionData, EventsData, AttemptCount, CreatedAt, NextRetryAt)
- Created SyncState enum with 3 states for UI sync indicator (Synced, Syncing, Offline)
- Extended ISessionRepository with 3 SyncQueue CRUD methods and implemented them in SessionRepository
- Extended ISessionSyncService with realtime lifecycle methods, sync state property, and sessions-downloaded event
- SyncQueue table auto-created in SessionRepository.InitializeAsync alongside existing tables

## Task Commits

Each task was committed atomically:

1. **Task 1: Create SyncQueueItem model and SyncState enum** - `9acb3d0` (feat)
2. **Task 2: Extend ISessionRepository with SyncQueue CRUD and update SessionRepository** - `8821f0a` (feat)
3. **Task 3: Extend ISessionSyncService with realtime lifecycle and sync state** - `ba47503` (feat)

## Files Created/Modified
- `AclsTracker/Models/SyncQueueItem.cs` - SQLite model for persisted retry queue with [Table("SyncQueue")]
- `AclsTracker/Services/Sync/SyncState.cs` - Sync state enum (Synced, Syncing, Offline)
- `AclsTracker/Services/Database/ISessionRepository.cs` - Added 3 SyncQueue CRUD methods
- `AclsTracker/Services/Database/SessionRepository.cs` - Creates SyncQueue table + implements CRUD methods
- `AclsTracker/Services/Sync/ISessionSyncService.cs` - Added realtime lifecycle, sync state, sessions-downloaded
- `AclsTracker/Services/Sync/SessionSyncService.cs` - Added stub implementations for new interface members

## Decisions Made
- Used raw SQL QueryAsync for GetPendingSyncItemsAsync (same pattern as GetSessionsByUserIdAsync) because sqlite-net-pcl TableQuery OrderBy may not work on all platforms
- SessionSyncService stubs throw NotImplementedException — Plan 02 will provide real implementations
- SyncQueueItem follows EventRecordEntity pattern: plain POCO, no base class, sqlite-net-pcl compatible

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Plan specified `net9.0-android` as build target but project uses `net9.0-android36.0` — adjusted build command accordingly (pre-existing project configuration, not a plan defect)

## Next Phase Readiness
- All data layer contracts ready for Plan 02 (service implementation: persistent retry queue, realtime sync)
- No breaking changes to existing consumers (MainViewModel, AuthViewModel, HistorialViewModel)
- SessionSyncService compiles with NotImplementedException stubs for new methods

---
*Phase: 06-cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry*
*Completed: 2026-04-02*

## Self-Check: PASSED

All 6 files verified on disk. All 3 task commits verified in git history (9acb3d0, 8821f0a, ba47503).
