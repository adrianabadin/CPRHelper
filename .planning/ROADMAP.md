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
- [ ] **Phase 5: Data Export** - Enable PDF and CSV export of session data

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
**Requirements**: EXPO-01, EXPO-02
**Success Criteria** (what must be TRUE):
  1. User can finalize a code and save session with patient data (nombre, apellido, DNI) to SQLite
  2. All events from session are persisted and associated via SessionId
  3. User can search past sessions by patient data (partial match) and date range
  4. User can view saved session detail with all events
  5. Saved sessions are immutable (no edit/delete)
**Plans:** 3 plans
Plans:
- [ ] 04-01-PLAN.md — SQLite database layer (models, repository, mapper, DI registration)
- [ ] 04-02-PLAN.md — Save flow (PatientDataPopup + MainViewModel.StopCode integration)
- [ ] 04-03-PLAN.md — HistorialPage evolution (search, session list, detail view)
**UI hint**: yes

### Phase 04.1: fix ui stuff (INSERTED)

**Goal:** [Urgent work - to be planned]
**Requirements**: TBD
**Depends on:** Phase 4
**Plans:** 0 plans

Plans:
- [ ] TBD (run /gsd:plan-phase 04.1 to break down)

### Phase 5: Data Export
**Goal**: Users can export recorded session data for documentation and review
**Depends on**: Phase 3
**Requirements**: EXPO-01, EXPO-02
**Success Criteria** (what must be TRUE):
  1. User can export session data in PDF format (readable report)
  2. User can export session data in CSV format (structured data)
**Plans**: TBD
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
| 4. Data Persistance | 0/3 | Planning complete | - |
| 4.1. Fix UI stuff | 0/0 | Not started | - |
| 5. Data Export | 0/0 | Not started | - |

---

*Roadmap created: 24/03/2026*
