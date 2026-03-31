# Phase 6 Context: Cloud PostgreSQL Sync

**Phase Goal:** Sync events to remote PostgreSQL database on save, with offline retry
**Created:** 2026-03-30
**Status:** Decisions captured, pending auth phase dependency

## Hard Dependencies

- **Authentication phase (TBD)** — Must be implemented BEFORE this phase. User login via Google Auth / Sign in with Apple provides:
  - User identity (which account owns the sessions)
  - Device ID generation (tied to authenticated user)
  - Required for bidirectional sync (know whose data to pull)

## Decisions

### 1. Sync Behavior on Save

| Decision | Choice |
|----------|--------|
| User feedback during sync | Invisible — no UI indicators |
| Failure notification | None — transparent to user |
| Retry strategy | Automatic on connectivity restore (monitor network state) |
| Pending sessions | Bulk upload all unsynced sessions when connection returns |
| Sync trigger | On save (attempt immediately), then retry on reconnect |

**Implementation implication:** Need a `SyncStatus` flag per session in local DB (e.g., `Pending`, `Synced`). Background connectivity listener triggers sync of all pending sessions.

### 2. PostgreSQL Data Model

| Decision | Choice |
|----------|--------|
| Table structure | Replicate Sessions + EventRecords from local SQLite |
| Device identification | Generate device_id tied to authenticated user |
| Patient data (nombre, apellido, DNI) | Plaintext — no encryption/anonymization beyond TLS in transit |
| Schema management | Migrations run from the app (code-first) |

**Implementation implication:** App needs migration capability for PostgreSQL. Connection string provided at runtime by user/config. Device ID generated on first login per device, persisted locally.

### 3. Sync Scope and Conflicts

| Decision | Choice |
|----------|--------|
| Sync direction | Bidirectional — push local→cloud AND pull cloud→local |
| Duplicate detection | By Session GUID — if exists in target, skip |
| Deletion propagation | No — cloud is append-only, local deletion does NOT delete from cloud |
| Cloud consumer | Backup/respaldo only (no dashboard or external system for now) |

**Implementation implication:** Pull sync downloads sessions from cloud that don't exist locally (e.g., from another device on same account). Conflict resolution is simple: GUID-based dedup, no merge needed since sessions are immutable.

## Code Context

### Current Data Layer (to integrate with)
- **Repository:** `ISessionRepository` / `SessionRepository` — singleton, SQLite via sqlite-net-pcl
- **Save flow:** `MainViewModel.StopCode()` → `PatientDataPopup` → `SessionRepository.SaveSessionAsync(session, events)`
- **Models:** `Session` + `EventRecord` (UI) / `EventRecordEntity` (DB) with `EventRecordMapper`
- **DI:** All registered in `MauiProgram.cs`
- **DB file:** `aclstracker.db3` in `FileSystem.Current.AppDataDirectory`

### Integration Points
- `SaveSessionAsync()` — after local save, trigger cloud sync attempt
- `App.xaml.cs` or startup — register connectivity listener for retry
- `HistorialViewModel` — may need to show cloud-synced sessions from pull

### New Components Needed
- Cloud sync service (push + pull logic)
- Connectivity monitor (detect online/offline transitions)
- PostgreSQL client library (Npgsql or similar)
- Migration runner for remote schema
- SyncStatus tracking in local SQLite (per session)
- Device ID generation and local persistence

## Deferred Ideas

- Dashboard web para consultar sesiones en la nube
- Sync selectivo (elegir qué sesiones sincronizar)
- Indicador visual de estado de sync en UI

---

*Context captured: 2026-03-30*
