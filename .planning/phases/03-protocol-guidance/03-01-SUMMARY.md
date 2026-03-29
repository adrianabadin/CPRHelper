---
phase: 03-protocol-guidance
plan: 01
subsystem: protocol-guidance
tags: [acls, aha-2020, rhythm-popup, pulse-check, cycle-counter, protocol-reminders]

# Dependency graph
requires:
  - phase: 02.1.1-event-recording-fixes
    provides: EventRecordingViewModel with CurrentRhythm, HsAndTsItems, LogCustomEventCommand
provides:
  - HandleRhythmChangeAsync with ACEPTAR/RECHAZAR popups for all 5 rhythms
  - Protocol guidance suggestions in pulse check popup
  - Cycle counter for first-cycle IV/IO reminder
affects: [04-export, testing]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Rhythm change popup guard pattern: _isPopupShowing flag prevents stacking"
    - "Protocol suggestion ordering: IV/IO → Rotar compresor → H's/T's → Adrenalina → Amiodarona"

key-files:
  created: []
  modified:
    - "AclsTracker/ViewModels/MainViewModel.cs"

key-decisions:
  - "All 5 rhythms (AESP, Asistolia, TV, FV, RCE) show ACEPTAR/RECHAZAR popups with decision logging"
  - "Old one-button TV/FV popup replaced with comprehensive protocol guidance for all rhythms"
  - "Protocol suggestions in pulse check ordered: IV/IO → Rotar compresor → H's/T's → medications"
  - "IV/IO reminder only on first cycle (_cycleCount == 0) since OnPulseCheckDue fires before NewCycle"

patterns-established:
  - "Popup guard: _isPopupShowing with try/finally prevents stacking on rapid rhythm changes"
  - "Decision logging: Recomendación {aceptada/rechazada}: {summary} format in event log"

requirements-completed:
  - REGI-04

# Metrics
duration: 5min
completed: 2026-03-29
---

# Phase 3 Plan 1: Protocol Guidance Summary

**Context-aware AHA ACLS 2020 protocol reminders via rhythm change popups with decision logging and pulse check suggestions (IV/IO, compressor rotation, pending H's and T's)**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-29T11:50:23Z
- **Completed:** 2026-03-29T11:55:49Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Added HandleRhythmChangeAsync with protocol-specific popups for all 5 cardiac rhythms (AESP, Asistolia, TV, FV, RCE) with ACEPTAR/RECHAZAR buttons and decision logging
- Extended pulse check popup with protocol guidance suggestions: IV/IO access (first cycle), compressor rotation (every cycle), pending H's and T's (dynamic)
- Added cycle counter (_cycleCount) tracking across StartCode/NewCycle for first-cycle logic
- Replaced old one-button "Ritmo Desfibrilable" popup with comprehensive rhythm-specific protocol guidance

## Task Commits

Each task was committed atomically:

1. **Task 1: Add rhythm change popups with HandleRhythmChangeAsync + cycle counter** - `5d25c0e` (feat)
2. **Task 2: Extend OnPulseCheckDue with protocol guidance suggestions** - `284ae76` (feat)

## Files Created/Modified
- `AclsTracker/ViewModels/MainViewModel.cs` - Added HandleRhythmChangeAsync, cycle counter, protocol suggestions in pulse check

## Decisions Made
- All 5 rhythms show ACEPTAR/RECHAZAR popups — not just TV/FV — for comprehensive protocol coverage
- Old "Ritmo Desfibrilable - Defibrile y reanude compresiones" one-button popup fully replaced
- IV/IO reminder uses _cycleCount == 0 because OnPulseCheckDue fires before NewCycle on first cycle
- Decision logging format: "Recomendación {aceptada/rechazada}: {first-line summary}"
- _isPopupShowing guard with try/finally prevents popup stacking during rapid rhythm changes

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Protocol guidance system complete with rhythm popups and pulse check suggestions
- Ready for Phase 04 (export) or further protocol enhancements
- Build passes with 0 errors on all target frameworks

## Self-Check: PASSED

- FOUND: AclsTracker/ViewModels/MainViewModel.cs
- FOUND: .planning/phases/03-protocol-guidance/03-01-SUMMARY.md
- FOUND: 5d25c0e (Task 1 commit)
- FOUND: 284ae76 (Task 2 commit)

---
*Phase: 03-protocol-guidance*
*Completed: 2026-03-29*
