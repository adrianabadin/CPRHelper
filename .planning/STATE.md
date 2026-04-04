---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: unknown
last_updated: "2026-04-04T09:23:24.024Z"
progress:
  total_phases: 15
  completed_phases: 13
  total_plans: 36
  completed_plans: 36
---

# STATE: ACLS Tracker

**Created:** 24/03/2026
**Last Updated:** 02/04/2026 after completing 06-03 plan

## Project Reference

**Core Value:** El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales

**Current Focus:** Phase 06 — cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry

## Current Position

Phase: 06 (cloud-postgresql-sync-sync-events-to-remote-database-on-save-with-offline-retry)
Plan: 3 of 3 (All plans complete — checkpoint Task 4 pending human verify)

## Performance Metrics

**Phase Completion:** 12/14 phases completed
**Requirement Coverage:** 11/11 v1 requirements mapped (100%)
**Plans Created:** 36
**Plans Completed:** 36

## Accumulated Context

### Key Decisions Logged

| Decision | Rationale | Phase |
|----------|-----------|-------|
| Phase structure derived from requirements (4 phases) | Natural delivery boundaries: timing → recording → guidance → export | Planning |
| All phases require UI components | Touch interface for emergency use conditions | Planning |
| QuestPDF namespace aliases for MAUI compatibility | IContainer and Colors conflict with MAUI implicit usings | 05-01 |
| QuestPDF 2026.x ColumnsDefinition API | Uses Action-based API, not chainable | 05-01 |
| UTF-8 BOM for CSV Spanish Excel compatibility | StreamWriter with UTF8Encoding(true) | 05-01 |
| Supabase session serialization using System.Text.Json | Gotrue internal serialization not public | 05.1-01 |
| OAuth callback uses aclstracker:// scheme | Matches Supabase OAuth redirect URI configuration | 05.1-01 |
| AuthViewModel singleton for shared auth state | Ensures consistent auth state across modal pages | 05.1-03 |
| UserProfile inherits from BaseModel for ORM | Required for Supabase Client.From<T>() to work | 05.1-02 |
| OAuth SignIn returns ProviderAuthState not string | ProviderAuthState.Uri contains the OAuth URL | 05.1-02 |
| Modal Shell navigation for auth pages | LoginPage, RegisterPage, ProfilePage via Routing.RegisterRoute | 05.1-03 |
| Phase 05.1-autenticacion-opcional-google-auth-apple-sign-in-email-password-con-verificacion P05.1-04-PLAN.md | 15m | 2 tasks | 6 files |
| UserId nullable on Session model | Supports orphan sessions saved before user logs in | 05.2-01 |
| GetSessionsByUserIdAsync uses raw SQL QueryAsync | sqlite-net-pcl TableQuery.OrderByDescending compatibility | 05.2-01 |
| InsertDownloadedSessionAsync skips existing IDs | Immutable sync — no overwrite of local data | 05.2-01 |
| Logout cleanup delegated to AuthViewModel (not OnAuthStateChanged) | CurrentUserId is null after SignOutAsync completes | 05.2-02 |
| IDispatcherTimer for retry timer | MAUI cross-platform UI-thread-safe timer API | 05.2-02 |
| Fire-and-forget upload in StopCode | _ = _syncService.UploadSessionAsync keeps UI non-blocking; retry queue handles failures | 05.2-03 |
| Logout cleanup captures CurrentUserId before SignOutAsync | CurrentUserId is null after sign-out, must capture first | 05.2-03 |
| Cloud indicator uses DataTrigger + global IsNotNullConverter | No new converter needed; IsNotNullConverter already in App.xaml | 05.2-03 |
| GetPendingSyncItemsAsync uses raw SQL QueryAsync | sqlite-net-pcl TableQuery OrderBy compatibility | 06-01 |
| SessionSyncService stubs throw NotImplementedException | Plan 02 will provide real implementations | 06-01 |
| SyncQueueItem plain POCO no base class | sqlite-net-pcl incompatible with CommunityToolkit source generators | 06-01 |
| Global sync indicator in toggle bar, per-session cloud stays green | Separates global connection state from per-session backup state | 06-03 |
| Toast uses Snackbar.Make with 3-second duration on MainThread | Auto-dismisses without blocking user, unlike DisplayAlert | 06-03 |
| StopRealtimeSync called first in logout before cleanup | Unsubscribes WebSocket before session deletion prevents race | 06-03 |
| Phase 06 P03 | 4min | 3 tasks | 3 files |

### Roadmap Evolution

- Phase 02.1.1 inserted after Phase 2.1: mejoras en defibrilacion y drogas
- Phase 02.1 inserted after Phase 2: modificar la estructura de la UI del proyecto
- Phase 3.1 added: Fix defibrillator notification, drug suggestions
- Phase 3.2 inserted after Phase 3: mejoras de UI
- Phase 03.3 inserted after Phase 3: Mejorar UI Hs y Ts
- Phase 5 added: Data export (PDF + CSV)
- Phase 05.1 added: Autenticacion opcional (Google, Apple, email/password con verificacion)
- Phase 05.2 inserted after Phase 5: Vincular sesiones de Supabase al usuario logueado (URGENT)
- Phase 07 added: modificar frontend para mostrar botones adrenalina y amiodarona en primera vista con layout optimizado

### Architecture Notes

- Platform: Android + iOS (cross-platform)
- Database: SQLite (encrypted, offline-first)
- Audio: Plugin.Maui.Audio
- UI: XAML with CommunityToolkit.Maui for animations
- Export: QuestPDF (PDF), StreamWriter with UTF-8 BOM (CSV)
- Auth: Supabase (email/password, Google OAuth, Apple Sign-In)
- Architecture: MVVM, Repository pattern, Offline-first

### Todos

**Immediate (Next Actions):**

- [ ] Proceed to next phase or milestone review

**Upcoming:**

- [ ] Complete v1.0 milestone

### Blockers

None identified.

### Known Issues

None identified.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 1 | set provided image as app icon | 2026-04-04 | 8cb3369 | [1-set-provided-image-as-app-icon](./quick/1-set-provided-image-as-app-icon/) |

## Session Continuity

**Last Session:** 2026-04-04T09:23:23.982Z
**Current Session:** Completed Phase 06 Plan 03 — Sync UI indicator, toast notifications, realtime lifecycle on logout

**Context Handoff:**

- Phase 06 Plan 03 complete (auto tasks): HistorialViewModel sync state + toast, HistorialPage multi-color indicator, AuthViewModel StopRealtimeSync
- Checkpoint Task 4 pending: human verification of realtime sync on device
- All 3 plans in Phase 06 have SUMMARYs — phase structurally complete

**Next Session Tasks:**

- Human verify Task 4 (checkpoint:human-verify) for Phase 06 Plan 03
- After approval: Phase 06 complete, proceed to Phase 07 or milestone review

---

*State updated: 2026-04-04 - Completed quick task 1: set provided image as app icon*
