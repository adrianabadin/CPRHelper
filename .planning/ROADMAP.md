# ROADMAP: ACLS Tracker

**Created:** 24/03/2026
**Phases:** 4
**Granularity:** Fine

## Phases

- [ ] **Phase 1: Metronome & Timers** - Establish core timing engine with audio metronome and multi-timer system
- [ ] **Phase 2: Event Recording** - Capture rhythm selection, event logging with timestamps, and H's/T's tracking
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
**Plans:** 4 plans
Plans:
- [x] 01-01-PLAN.md — Project scaffold, models, service interfaces, DI setup
- [x] 01-02-PLAN.md — Audio metronome engine (AudioService + MetronomeService + ViewModel)
- [x] 01-03-PLAN.md — Multi-timer system (TimerService + TimerViewModel)
- [ ] 01-04-PLAN.md — UI integration (MainPage, MetronomePulse, TimerCard controls)
**UI hint**: yes

### Phase 2: Event Recording
**Goal**: Users can document all events and decisions during resuscitation events
**Depends on**: Phase 1
**Requirements**: REGI-01, REGI-02, REGI-03
**Success Criteria** (what must be TRUE):
  1. User can select current cardiac rhythm (FV/TV, AEA, Asistolia, Bradicardia, Taquicardia)
  2. System automatically logs all events with timestamps (including milliseconds)
  3. User can mark/dismiss items from H's and T's checklist
**Plans**: TBD
**UI hint**: yes

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
| 1. Metronome & Timers | 3/4 | In Progress | - |
| 2. Event Recording | 0/0 | Not started | - |
| 3. Protocol Guidance | 0/0 | Not started | - |
| 4. Data Export | 0/0 | Not started | - |

---

*Roadmap created: 24/03/2026*
