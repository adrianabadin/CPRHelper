# ROADMAP: ACLS Tracker

**Created:** 24/03/2026
**Phases:** 4
**Granularity:** Fine

## Phases

- [x] **Phase 1: Metronome & Timers** - Establish core timing engine with audio metronome and multi-timer system (completed 2026-03-24)
- [x] **Phase 2: Event Recording** - Capture rhythm selection, event logging with timestamps, and H's/T's tracking (completed 2026-03-25)
- [ ] **Phase 3: Protocol Guidance** - Generate context-aware reminders based on AHA ACLS 2020 protocol
- [ ] **Phase 4: Data Persistance** - Allow User to persist sessions in local Database
- [x] **Phase 3.1: Fix design issues** - Fix defibrillator notification, rhythm popup UX, and drug suggestions (completed 2026-03-29)
- [x] **Phase 5: Data Export** - Enable PDF and CSV export of session data (completed 2026-03-30)

## Phase Details

### Phase 1: Metronome & Timers
**Goal**: Users can manage core timing functions for cardiac resuscitation guidance
**Depends on**: Nothing
**Requirements**: AUDI-01, AUDI-02, TIME-01, TIME-02, TIME-03
**Success Criteria** (what must be TRUE):
  1. User can start audio metronome at configurable BPM (100-120)
  2. User sees visual metronome animation synchronized with audio (60fps)
  3. User can manage multiple concurrent timers (2-min cycle + medication timers)
  4. User can independently start, pause, and restart each timer
  5. User sees elapsed time in real-time for all active timers
**Plans:** 4/4 plans complete
Plans:
- [x] 01-01-PLAN.md — Project scaffold, models, service interfaces, DI setup
- [x] 01-02-PLAN.md — Audio metronome engine (AudioService + MetronomeService + ViewModel)
- [x] 01-03-PLAN.md — Multi-timer system (TimerService + TimerViewModel)
- [x] 01-04-PLAN.md — UI integration (MainPage, MetronomePulse, TimerCard controls)
**UI hint**: yes

### Phase 2: Event Recording
**Goal**: Users can document all events and decisions during resuscitation events
**Depends on**: Phase 1
**Requirements**: REGI-01, REGI-02, REGI-03
**Success Criteria** (what must be TRUE):
  1. User can select current cardiac rhythm (FV/TV, AEA, Asistolia, Bradicardia, Taquicardia)
  2. System automatically logs all events with timestamps (including milliseconds)
  3. User can mark/dismiss items from H's and T's checklist
**Plans:** 2/2 plans complete
Plans:
- [ ] 02-01-PLAN.md — Models, EventLogService, and EventRecordingViewModel
- [ ] 02-02-PLAN.md — UI controls (RhythmSelector, HsAndTsChecklist, EventLogPanel) and MainPage integration
**UI hint**: yes

### Phase 02.1: modificar la estructura de la UI del proyecto (INSERTED)
**Goal:** Reorganize app UI structure for emergency use: 3-tab Shell navigation, compact above-the-fold layout with all critical controls, 6 timers, new action buttons, and 2-minute pulse-check popup
**Requirements**: REGI-01, REGI-02, REGI-03, AUDI-02, TIME-01, TIME-02, TIME-03
**Depends on:** Phase 2
**Plans:** 4/4 plans complete
Plans:
- [x] 02.1-01-PLAN.md — Enums, TimerModel, TimerViewModel 6-timer init, compact TimerCard
- [x] 02.1-02-PLAN.md — Compact MetronomePulse, RhythmSelector, HsAndTsPage, HistorialPage, 3-tab AppShell
- [x] 02.1-03-PLAN.md — MainPage 6-block layout, MainViewModel action commands and pulse-check popup
- [ ] 02.1-04-PLAN.md — Gap closure: ventilation text in popup, defibrillation popup, compressions timer FCT fix

### Phase 02.1.1: mejoras en defibrilacion y drogas: registro de decision, alertas rojas >4min, sugerencias de medicamentos (INSERTED)

**Goal:** Mejorar funcionalidad existente de Phase 2.1 agregando registro de defibrilación manual, alertas visuales de tiempo crítico, y sugerencias de medicamentos en popup de check de pulso
**Requirements**: TBD (extends REGI-02, improves existing functionality)
**Depends on:** Phase 2.1
**Plans:** 3/3 plans complete

Plans:
- [ ] 02.1.1-01-PLAN.md — Defibrilation button and event registration (UI + MainViewModel)
- [ ] 02.1.1-02-PLAN.md — Red alerts for timers exceeding thresholds (TimerModel + TimerViewModel + TimerCard)
- [ ] 02.1.1-03-PLAN.md — Medication suggestions in pulse-check popup (MainViewModel)

### Phase 3: Protocol Guidance
**Goal**: Users receive context-aware reminders based on AHA ACLS 2020 protocol during resuscitation — rhythm change popups with ACEPTAR/RECHAZAR decision logging, plus protocol checklist suggestions (IV/IO, compressor rotation, H's and T's) during 2-minute pulse checks
**Depends on**: Phase 2
**Requirements**: REGI-04
**Success Criteria** (what must be TRUE):
  1. Changing rhythm to AESP/Asistolia shows reversible causes popup with ACEPTAR/RECHAZAR
  2. Changing rhythm to TV/FV shows defibrillation + H's and T's popup (replaces old one-button popup)
  3. Changing rhythm to RCE shows post-ROSC checklist popup
  4. ACEPTAR/RECHAZAR decisions are logged to event log
  5. Pulse check popup includes IV/IO (first cycle), Rotar compresor (every cycle), pending H's and T's
**Plans:** 1 plan
Plans:
- [ ] 03-01-PLAN.md — Rhythm change popups + pulse check suggestions + cycle counter (MainViewModel.cs)
**UI hint**: yes

### Phase 03.3: Mejorar UI Hs y Ts mobile first tab Causas Reversibles (INSERTED)

**Goal:** Improved mobile-first UI for HsAndTsChecklist control — single column, 48x48 round buttons, full-row tap, 16pt font, tab renamed to "Causas Reversibles"
**Requirements**: REGI-03 (UI improvement, no new functionality)
**Depends on:** Phase 3
**Plans:** 1/1 plans complete

Plans:
- [ ] 03.3-01-PLAN.md — Redesign HsAndTsChecklist mobile-first UI (single column, 48x48 buttons, TapGestureRecognizer, FontSize=16)

### Phase 03.2: UI improvements - pulse checks, compression pause, new code session, bullets, reversible causes, defibrillation animation, banner (INSERTED)

**Goal:** Rediseno del popup de check de pulso como modal personalizado que permanece abierto, pausa automatica de compresiones y metronomo durante el check, opcion de continuar/reiniciar codigo, animacion de defibrilacion con haptic, vinetas en popups de ritmo con "Valorar Causas Reversibles", y banners de 8 segundos
**Requirements**: AUDI-02, TIME-02, REGI-02, REGI-04
**Depends on:** Phase 3
**Plans:** 3/3 plans complete

Plans:
- [ ] 03.2-01-PLAN.md — Timer/metronome pause infrastructure, TimerCard EN PAUSA + pause button, banner 8s, rhythm popup bullets, defibrillation animation
- [ ] 03.2-02-PLAN.md — Custom PulseCheckPage modal, OnPulseCheckDue rewrite, INICIAR CODIGO continue/new code flow

### Phase 3.1: Fix defibrillator notification, rhythm popup UX, and drug suggestions (INSERTED)

**Goal:** Corregir 5 problemas de diseño: 1) notificación de preparar desfibrilador no aparece (bug: Shell navigation), 2) remover botón DEFIBRILAR del popup de check de pulso, 3) popup TV/FV → solo OK (no ACEPTAR/RECHAZAR), 4) sugerencias de medicación basadas en protocolo ACLS (ciclo+ritmo) con botones destacados en UI, 5) reset de timers al finalizar/iniciar código
**Requirements**: REGI-04
**Depends on:** Phase 3
**Plans:** 2/2 plans complete

Plans:
- [ ] 03.1-01-PLAN.md — Fix ShowNotification Shell bug, simplify pulse check popup, TV/FV single OK, timer reset on StopCode/StartCode
- [ ] 03.1-02-PLAN.md — ACLS-protocol medication suggestions, >4min banner, highlighted drug buttons in UI

### Phase 4: Data Persistance
**Goal**: Users can persist session data in local database, search past sessions by patient data and date, and view session details
**Depends on**: Phase 3
**Requirements**: PERS-01
**Success Criteria** (what must be TRUE):
  1. User can finalize a code and save session with patient data (nombre, apellido, DNI) to SQLite
  2. All events from session are persisted and associated via SessionId
  3. User can search past sessions by patient data (partial match) and date range
  4. User can view saved session detail with all events
  5. Saved sessions are immutable (no edit/delete)
**Plans:** 4 plans
Plans:
- [ ] 04-01-PLAN.md — SQLite database layer (models, repository, mapper, DI registration)
- [ ] 04-02-PLAN.md — Save flow (PatientDataPopup + MainViewModel.StopCode integration)
- [ ] 04-03-PLAN.md — HistorialPage evolution (search, session list, detail view)
- [ ] 04-04-PLAN.md — Gap closure: DatePicker binding fix + requirement traceability (PERS-01)
**UI hint**: yes

### Phase 5: Data Export
**Goal**: Users can export session data as PDF (readable clinical report) and CSV (structured event log) from the saved session detail view, with share sheet and local file save
**Depends on**: Phase 4
**Requirements**: EXPO-01, EXPO-02
**Success Criteria** (what must be TRUE):
  1. User can export session data in PDF format (readable report with 6 clinical sections)
  2. User can export session data in CSV format (structured data with 6 columns, UTF-8 BOM)
**Plans:** 2/2 plans complete
Plans:
- [x] 05-01-PLAN.md — Export services: QuestPDF PDF generation + CSV generation + DI registration
- [x] 05-02-PLAN.md — ViewModel export commands + UI buttons + human verification
**UI hint**: yes

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Metronome & Timers | 4/4 | Complete    | 2026-03-24 |
| 2. Event Recording | 2/2 | Complete   | 2026-03-25 |
| 02.1. UI Restructure | 4/4 | Complete    | 2026-03-28 |
| 3. Protocol Guidance | 0/0 | Not started | - |
| 3.1. Fix design issues | 2/2 | Complete   | 2026-03-29 |
| 03.2. UI improvements | 0/2 | Complete    | 2026-03-30 |
| 4. Data Persistance | 2/3 | In Progress|  |
| 4.1. Fix UI stuff | 0/0 | Not started | - |
| 5. Data Export | 2/2 | Complete | 2026-03-30 |
| 05.1. Auth (opcional) | 4/4 | Complete   | 2026-03-31 |
| 05.2. Session Sync | 3/3 | Complete   | 2026-03-31 |
| 06. Cloud PostgreSQL Sync | 1/3 | In Progress |  |

### Phase 05.2: Vincular sesiones de Supabase al usuario logueado (INSERTED)

**Goal:** Asociar sesiones de reanimación guardadas en SQLite local al usuario autenticado en Supabase, con upload al guardar, download al login, borrado local al logout, e indicador visual de respaldo en la nube
**Requirements**: SYNC-01
**Depends on:** Phase 05.1
**Plans:** 3/3 plans complete

Plans:
- [ ] 05.2-01-PLAN.md — Data Layer: Models (UserId, SessionSupabase, EventSupabase), Repository extensions, Auth CurrentUserId
- [ ] 05.2-02-PLAN.md — SessionSyncService: upload, download, claim orphans, cleanup, retry queue, AuthStateChanged
- [ ] 05.2-03-PLAN.md — ViewModel integration + cloud indicator UI + human verification

### Phase 05.1: Autenticacion opcional - Google Auth, Apple Sign-In, email+password con verificacion (INSERTED)

**Goal:** Optional authentication with Google Auth, Apple Sign-In, and email+password to enable cloud backup (Phase 6). Login is optional — app works fully without it. Auth widget in top-right of app bar.
**Requirements**: AUTH-INFRA, AUTH-EMAIL, AUTH-OAUTH, AUTH-SESSION, AUTH-PROFILE, AUTH-UI
**Depends on:** Phase 5
**Plans:** 4/4 plans complete

Plans:
- [x] 05.1-01-PLAN.md — Supabase infrastructure: NuGet, IAuthService interface, session handler, platform configs
- [x] 05.1-02-PLAN.md — AuthService implementation + Supabase DI registration + session restore
- [x] 05.1-03-PLAN.md — AuthViewModel + LoginPage + RegisterPage + ProfilePage
- [x] 05.1-04-PLAN.md — AuthAvatarControl in AppShell + human verification

### Phase 06: Cloud PostgreSQL sync - sync events to remote database on save with offline retry

**Goal:** Persistent retry queue with offline support, realtime sync via Supabase Realtime, and UI sync indicator
**Requirements**: SYNC-01
**Depends on:** Phase 05.2
**Plans:** 3/3 plans complete

Plans:
- [x] 06-01-PLAN.md — Data Layer Contracts: SyncQueueItem model, SyncState enum, extended interfaces
- [x] 06-02-PLAN.md — SessionSyncService implementation: persistent retry queue, realtime sync
- [x] 06-03-PLAN.md — UI sync indicator and retry queue visualization

### Phase 7: modificar frontend para mostrar botones adrenalina y amiodarona en primera vista con layout optimizado

**Goal:** Compress MainPage vertical layout so ADRENALINA and AMIODARONA drug buttons are visible without scrolling on standard Android screens, by collapsing rhythm selector to single row, merging action buttons into 2x2 grid, and reducing control sizes
**Requirements**: UI-LAYOUT-01
**Depends on:** Phase 6
**Plans:** 1 plan

Plans:
- [ ] 07-01-PLAN.md — Compress 4 XAML controls: RhythmSelector single-row Grid, MainPage 2x2 action grid, MetronomePulse circle reduction, TimerCard padding reduction

### Phase 8: Fix main UI layout issues - overlay defibrillation banner, compress timer cards, remove cardiac rhythm label

**Goal:** Recover vertical space on MainPage by making the defibrillation banner overlay content, compressing TimerCard to uniform 3-row layout with T.Comp rename, and removing RITMO CARDIACO header with active rhythm button white border highlight
**Requirements**: UI-08-A, UI-08-B, UI-08-C, UI-08-D, UI-08-E
**Depends on:** Phase 7
**Plans:** 1 plan

Plans:
- [ ] 08-01-PLAN.md — Banner overlay, TimerCard 3-row compression, rhythm selector cleanup with active highlight

### Phase 9: Fix UI issues and authentication: Google auth redirect loop, persistent login, button sizing and colors (on/off toggle, metronome +/-, start/end code button, non-defib rhythms color)

**Goal:** Fix 6 discrete bugs: Google OAuth redirect loop, session persistence across restarts, metronome ON/OFF toggle label, metronome button sizing, INICIAR/FINALIZAR button color/height, and non-defibrillable rhythm button colors
**Requirements**: P09-AUTH-01, P09-AUTH-02, P09-UI-01, P09-UI-02, P09-UI-03, P09-UI-04
**Depends on:** Phase 8
**Plans:** 3 plans

Plans:
- [ ] 09-01-PLAN.md — Fix Google OAuth PKCE flow + session persistence (2 human checkpoints)
- [ ] 09-02-PLAN.md — Fix metronome toggle ON/OFF label + reduce +/- button sizing
- [ ] 09-03-PLAN.md — Fix INICIAR/FINALIZAR color+height + AESP/ASISTOLIA yellow buttons

### Phase 10: Agregar datos extra en timecards: numero de ciclo, fraccion de compresion en T.Comp, numero de dosis en adrenalina y amiodarona (completed 2026-04-11)

**Goal:** Display secondary protocol metrics (cycle number, FCT%, dose counts) on existing TimerCard controls in subtle style without increasing card height
**Requirements**: ENH-10
**Depends on:** Phase 9
**Plans:** 1/1 plans complete

Plans:
- [x] 10-01-PLAN.md — ExtraInfo property on TimerModel, subtle Label on TimerCard, MainViewModel integration (cycle/FCT/dose counters)

### Phase 11: UI/UX redesign - new color palette, tooltips, and improved navigability without changing layout dimensions

**Goal:** Centralize clinical color palette in semantic resources, remove EventLogPanel from MainPage, add collapsible tab bar, and implement tooltip system with protocol info icons
**Requirements**: UX-11-PALETTE, UX-11-COLORS, UX-11-NAV-EVENTLOG, UX-11-NAV-TABBAR, UX-11-TOOLTIPS
**Depends on:** Phase 10
**Plans:** 3 plans

Plans:
- [ ] 11-01-PLAN.md — Centralize color palette in Colors.xaml, replace all inline hex with semantic resources
- [ ] 11-02-PLAN.md — Remove EventLogPanel from MainPage, add collapsible tab bar with floating toggle
- [ ] 11-03-PLAN.md — TooltipOverlay control + info icons on timers, rhythm, action buttons

---

*Roadmap created: 24/03/2026*
