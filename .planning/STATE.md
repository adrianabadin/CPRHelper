---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: unknown
last_updated: "2026-04-11T21:44:17.880Z"
progress:
  total_phases: 18
  completed_phases: 17
  total_plans: 42
  completed_plans: 42
---

# STATE: ACLS Tracker

**Created:** 24/03/2026
**Last Updated:** 11/04/2026 after completing Phase 10 Plan 01 (extra info on timer cards)

## Project Reference

**Core Value:** El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales

**Current Focus:** Phase 10 — agregar-datos-extra-en-timecards

## Current Position

Phase: 10 (agregar-datos-extra-en-timecards-numero-de-ciclo-fraccion-de-compresion-en-t-comp-numero-de-dosis-en-adrenalina-y-amiodarona)
Plan: 1 of 1 complete — extra info on timer cards (cycle number, FCT%, dose counts)
Verification: build passed, device testing recommended

## Performance Metrics

**Phase Completion:** 17/18 phases completed
**Requirement Coverage:** 17/17 phase 09 requirements mapped (100%)
**Plans Created:** 42
**Plans Completed:** 42

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
| Phase 07 P01 | 2min | 3 tasks | 4 files |
| Phase 08 P01 | 12min | 3 tasks | 7 files |
| Phase 09-fix-ui-issues-and-authentication P01 | 3min | 3 tasks | 3 files |
| Phase 09-fix-ui-issues-and-authentication P02 | 5min | 2 tasks | 3 files |
| Phase 09-fix-ui-issues-and-authentication P02 | 5min | 2 tasks | 3 files |
| Phase 09 P09-02 | 5min | 2 tasks | 3 files |
| Phase 09 P09-03 | 3min | 2 tasks | 2 files |
| PKCE OAuth requires explicit SignInOptions with FlowType=PKCE and RedirectTo | Prevents localhost:3000 fallback redirect | 09-01 |
| LoadSession() + RetrieveSessionAsync startup chain after SetPersistence | SetPersistence registers handler but never reads from storage | 09-01 |
| BoolToOnOffConverter for toggle labels instead of StringFormat | StringFormat '{0}' outputs True/False, not ON/OFF | 09-02 |
| Non-shockable rhythms use yellow #FBC02D with dark text | Visual grouping distinct from shockable (red) and normal (green); WCAG AA ~10:1 | 09-03 |
| ExtraInfo as ObservableProperty on TimerModel | Enables direct XAML binding without converters | 10-01 |
| FCT calculated via PropertyChanged subscriptions (not TimerService) | Keeps service decoupled from VM logic | 10-01 |
| Phase 10 P10-01 | 5min | 2 tasks | 3 files |

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
- Phase 8 added: Fix main UI layout issues - overlay defibrillation banner, compress timer cards, remove cardiac rhythm label
- Phase 9 added: Fix UI issues and authentication (Google auth redirect loop, persistent login, button sizing/colors)
- Phase 10 added: Agregar datos extra en timecards: numero de ciclo, fraccion de compresion en T.Comp, numero de dosis en adrenalina y amiodarona
- Phase 11 added: UI/UX redesign - new color palette, tooltips, and improved navigability without changing layout dimensions

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

- [ ] Device verification: Google OAuth login + session persistence (Phase 09 Plan 01 checkpoints)
- [ ] Device verification: UI changes on emulator (metronome ON/OFF, button colors/sizes)

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
| 2 | animate metronome circle with heartbeat pulse | 2026-04-04 | c940788 | [2-animar-circulo-metronomo-con-pulso-visua](./quick/2-animar-circulo-metronomo-con-pulso-visua/) |

## Session Continuity

**Last Session:** 2026-04-11T21:20:00.000Z
**Current Session:** Completed Phase 10 Plan 01 — extra info on timer cards (cycle number, FCT%, dose counts)

**Context Handoff:**

- Phase 10 Plan 01 complete: ExtraInfo infrastructure on TimerModel with cycle number, FCT%, and dose counts
- TimerModel has ExtraInfo (string) and IsExtraInfoVisible (bool) ObservableProperty fields
- TimerCard.xaml shows subtle 12pt #999999 Label to the right of elapsed time via HorizontalStackLayout
- MainViewModel wires: NewCycle→Timers[1] cycle number, Adrenalina→Timers[3] dose count, Amiodarona→Timers[4] dose count, FCT→Timers[2] via PropertyChanged
- ResetCodeState clears all ExtraInfo and resets _adrenalinaDoseCount
- Both tasks have SUMMARY.md with Self-Check: PASSED

**Next Session Tasks:**

- Device testing: verify ExtraInfo labels visible and styled correctly on emulator
- Verify FCT% updates in real-time during compressions
- Verify all extra data clears on NUEVO CODIGO, persists on CONTINUAR
- Phase 10 complete, proceed to milestone review or next phase

---

*State updated: 2026-04-11 - Completed Phase 10 Plan 01: extra info on timer cards (cycle number, FCT%, dose counts)*
