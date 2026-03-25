---
phase: 02-event-recording
plan: 01
subsystem: recording
tags: [csharp, maui, mvvm, communitytoolkit, observablecollection, acls, event-log]

# Dependency graph
requires:
  - phase: 01-metronome-timers
    provides: ITimerService, TimerViewModel, MVVM patterns with CommunityToolkit.Mvvm

provides:
  - CardiacRhythm enum with 7 AHA ACLS 2020 rhythms
  - EventRecord model with millisecond-precision DateTime timestamps
  - HsAndTsItem observable model with checked/dismissed states
  - IEventLogService interface and EventLogService implementation
  - EventRecordingViewModel with rhythm selection, H&T management, and event logging commands

affects:
  - 02-event-recording plan 02 (UI wiring for event recording)
  - 04-export (event log data consumed for PDF/CSV export)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Thread-safe ObservableCollection with lock pattern (same as TimerService)"
    - "Newest-first event log insertion (Insert at index 0)"
    - "Singleton service + Transient ViewModel DI registration pattern"

key-files:
  created:
    - AclsTracker/Models/CardiacRhythm.cs
    - AclsTracker/Models/EventRecord.cs
    - AclsTracker/Models/HsAndTsItem.cs
    - AclsTracker/Services/EventLog/IEventLogService.cs
    - AclsTracker/Services/EventLog/EventLogService.cs
    - AclsTracker/ViewModels/EventRecordingViewModel.cs
  modified:
    - AclsTracker/MauiProgram.cs

key-decisions:
  - "EventLogService inserts events at index 0 (newest first) to match expected UI display order"
  - "Thread-safe event insertion using lock object, consistent with TimerService pattern"
  - "FibrilacionVentricular and TaquicardiaVentricular both map to display name FV/TV per AHA ACLS convention"
  - "SessionStartTime nullable — ElapsedSinceStart defaults to TimeSpan.Zero if session not started"

patterns-established:
  - "Event categories: RhythmChange, Medication, CprCycle, HsTs, Custom, Session"
  - "Spanish language for all user-facing event descriptions (consistent with AHA ACLS Spanish localization)"
  - "H's and T's IDs follow kebab-case: h-hipovolemia, t-neumo, etc."

requirements-completed: [REGI-01, REGI-02, REGI-03]

# Metrics
duration: 6min
completed: 2026-03-25
---

# Phase 02 Plan 01: Event Recording — Data Models, Service, and ViewModel Summary

**CardiacRhythm enum (7 ACLS rhythms), EventRecord with millisecond timestamps, HsAndTsItem observable model, thread-safe EventLogService with newest-first ordering, and EventRecordingViewModel with 6 relay commands — all registered in DI**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-25T13:08:14Z
- **Completed:** 2026-03-25T13:14:03Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- CardiacRhythm enum with all 7 AHA ACLS 2020 rhythms (Ninguno, FV, TV, AEA, Asistolia, Bradicardia, Taquicardia) satisfies REGI-01
- EventRecord model with `DateTime.Now` millisecond-precision timestamps and relative `ElapsedSinceStart` satisfies REGI-02
- HsAndTsItem with `IsChecked`, `IsDismissed`, `CheckedAt` observable properties for full REGI-03 checklist tracking
- Thread-safe `EventLogService` with newest-first event insertion and session lifecycle management
- `EventRecordingViewModel` with 6 relay commands: `SelectRhythm`, `ToggleHsAndTsItem`, `DismissHsAndTsItem`, `LogCustomEvent`, `StartRecording`, `StopRecording`
- All 10 H's and T's initialized in Spanish in constructor (5 H's, 5 T's per AHA ACLS 2020)
- `IEventLogService` and `EventRecordingViewModel` registered in DI (Singleton + Transient)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create data models (CardiacRhythm, EventRecord, HsAndTsItem)** - `19b2d0d` (feat)
2. **Task 2: Create EventLogService, EventRecordingViewModel, and register in DI** - `38d37da` (feat)

**Plan metadata:** (docs commit — see below)

## Files Created/Modified

- `AclsTracker/Models/CardiacRhythm.cs` — 7-value enum for AHA ACLS 2020 cardiac rhythms
- `AclsTracker/Models/EventRecord.cs` — ObservableObject model with millisecond DateTime, ElapsedSinceStart, EventType, Description, Details
- `AclsTracker/Models/HsAndTsItem.cs` — ObservableObject with IsChecked/IsDismissed/CheckedAt observable properties
- `AclsTracker/Services/EventLog/IEventLogService.cs` — Service contract with ObservableCollection<EventRecord>, session management, LogEvent
- `AclsTracker/Services/EventLog/EventLogService.cs` — Thread-safe implementation: lock on Insert/Clear, newest-first ordering, GUID IDs
- `AclsTracker/ViewModels/EventRecordingViewModel.cs` — ViewModel with rhythm selection (Spanish display names), H&T toggle/dismiss, custom event logging, session start/stop
- `AclsTracker/MauiProgram.cs` — Added IEventLogService Singleton + EventRecordingViewModel Transient registrations

## Decisions Made

- `FibrilacionVentricular` and `TaquicardiaVentricular` both map to display name "FV/TV" — per AHA ACLS 2020 convention they share the same shockable rhythm treatment algorithm
- `SessionStartTime` is nullable; `ElapsedSinceStart` defaults to `TimeSpan.Zero` if `LogEvent` is called before `StartSession()` — defensive design for partial usage
- Events inserted at index 0 (newest first) matching expected timeline-reversed log display in the UI
- Thread-safe `lock` on `Events.Insert/Clear` follows the same pattern established by `TimerService`

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All backend logic for Phase 2 event recording is complete and builds cleanly (0 errors on Android target)
- Ready for Plan 02-02: wire `EventRecordingViewModel` to UI (rhythm buttons, event log display, H&T checklist)
- `ITimerService` dependency in `EventRecordingViewModel` is injected but not yet used — reserved for Plan 02-02 UI to show elapsed timer values alongside events

---
*Phase: 02-event-recording*
*Completed: 2026-03-25*
