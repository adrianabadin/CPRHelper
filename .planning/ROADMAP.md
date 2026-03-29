# ROADMAP: ACLS Tracker

**Created:** 24/03/2026
**Phases:** 4
**Granularity:** Fine

## Phases

- [x] **Phase 1: Metronome & Timers** - Establish core timing engine with audio metronome and multi-timer system (completed 2026-03-24)
- [x] **Phase 2: Event Recording** - Capture rhythm selection, event logging with timestamps, and H's/T's tracking (completed 2026-03-25)
- [ ] **Phase 3: Protocol Guidance** - Generate context-aware reminders based on AHA ACLS 2020 protocol
- [ ] **Phase 4: Data Export** - Enable PDF and CSV export of session data

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
**Plans:** 1/3 plans executed

Plans:
- [ ] 02.1.1-01-PLAN.md — Defibrilation button and event registration (UI + MainViewModel)
- [ ] 02.1.1-02-PLAN.md — Red alerts for timers exceeding thresholds (TimerModel + TimerViewModel + TimerCard)
- [ ] 02.1.1-03-PLAN.md — Medication suggestions in pulse-check popup (MainViewModel)

### Phase 3: Protocol Guidance
**Goal**: Users receive context-aware reminders based on AHA ACLS 2020 protocol during resuscitation
**Depends on**: Phase 2
**Requirements**: REGI-04
**Success Criteria** (what must be TRUE):
  1. System generates audio reminders based on elapsed time and current rhythm
  2. User can acknowledge or dismiss protocol reminders during session
**Plans**: TBD
**UI hint**: yes

### Phase 4: Data Export
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
| 4. Data Export | 0/0 | Not started | - |

---

*Roadmap created: 24/03/2026*
