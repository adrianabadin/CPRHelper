---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: unknown
last_updated: "2026-03-31T23:06:00.000Z"
progress:
  total_phases: 14
  completed_phases: 11
  total_plans: 33
  completed_plans: 32
---

# STATE: ACLS Tracker

**Created:** 24/03/2026
**Last Updated:** 31/03/2026 after completing 05.2-01 plan

## Project Reference

**Core Value:** El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales

**Current Focus:** Phase 05.2 — vincular-sesiones-de-supabase-al-usuario-logueado

## Current Position

Phase: 05.2 (vincular-sesiones-de-supabase-al-usuario-logueado) — IN PROGRESS
Plan: 2 of 3 (Plan 02 complete)

## Performance Metrics

**Phase Completion:** 10/11 phases completed
**Requirement Coverage:** 11/11 v1 requirements mapped (100%)
**Plans Created:** 33
**Plans Completed:** 31

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

### Roadmap Evolution

- Phase 02.1.1 inserted after Phase 2.1: mejoras en defibrilacion y drogas
- Phase 02.1 inserted after Phase 2: modificar la estructura de la UI del proyecto
- Phase 3.1 added: Fix defibrillator notification, drug suggestions
- Phase 3.2 inserted after Phase 3: mejoras de UI
- Phase 03.3 inserted after Phase 3: Mejorar UI Hs y Ts
- Phase 5 added: Data export (PDF + CSV)
- Phase 05.1 added: Autenticacion opcional (Google, Apple, email/password con verificacion)
- Phase 05.2 inserted after Phase 5: Vincular sesiones de Supabase al usuario logueado (URGENT)

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

## Session Continuity

**Last Session:** 2026-03-31T23:06:00Z
**Current Session:** Completed Phase 05.2 Plan 02 — SessionSyncService with full orchestration

**Context Handoff:**

- SessionSyncService singleton registered in DI, subscribes to AuthStateChanged
- Login sync: claim orphans -> download user sessions -> drain retry queue
- Retry queue: exponential backoff (30s->300s cap), max 5 attempts per item, IDispatcherTimer
- Logout cleanup must be triggered from AuthViewModel (not from OnAuthStateChanged)
- SyncCompleted event available for HistorialViewModel to refresh session list
- UploadSessionAsync ready for EventRecordingViewModel to call after session save

**Next Session Tasks:**

- Proceed to Phase 05.2 Plan 03: wire up ViewModels to SessionSyncService.

---

*State updated: 31/03/2026*
