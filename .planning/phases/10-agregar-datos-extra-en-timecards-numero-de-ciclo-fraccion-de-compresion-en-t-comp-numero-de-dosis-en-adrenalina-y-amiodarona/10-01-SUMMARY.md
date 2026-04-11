---
phase: 10-agregar-datos-extra-en-timecards-numero-de-ciclo-fraccion-de-compresion-en-t-comp-numero-de-dosis-en-adrenalina-y-amiodarona
plan: 01
subsystem: ui
tags: [maui, xaml, mvvm, timer, data-binding]

requires:
  - phase: 09-fix-ui-issues-and-authentication
    provides: TimerModel, TimerCard, MainViewModel base with timers and drug logic

provides:
  - ExtraInfo and IsExtraInfoVisible observable properties on TimerModel
  - HorizontalStackLayout in TimerCard showing elapsed time + subtle extra info
  - Cycle number display on Ciclo RCP timer
  - FCT% calculation and display on T.Comp timer
  - Adrenalina and Amiodarona dose count display
  - ExtraInfo clearing on NUEVO CODIGO, persistence on CONTINUAR

affects:
  - Any future phase modifying TimerModel, TimerCard, or MainViewModel drug/timer logic

tech-stack:
  added: []
  patterns: [ObservableProperty for extra info binding, PropertyChanged subscriptions for reactive FCT calculation]

key-files:
  created: []
  modified:
    - AclsTracker/Models/TimerModel.cs
    - AclsTracker/Controls/TimerCard.xaml
    - AclsTracker/ViewModels/MainViewModel.cs

key-decisions:
  - "ExtraInfo as ObservableProperty on TimerModel enables direct XAML binding without converters"
  - "FCT calculated via PropertyChanged subscriptions on Timers[0].Elapsed and Timers[2].IsRunning (not inside TimerService)"
  - "HorizontalStackLayout preserves existing elapsed time behavior while adding extra info to the right"
  - "FCT hidden when compressions paused/stopped (pulse check, code stopped)"
  - "Integer format F0 for FCT% per user requirement (solo el numero + %)"

patterns-established:
  - "ExtraInfo pattern: ObservableProperty string + bool on TimerModel, direct assignment in MainViewModel, conditional XAML Label"

requirements-completed:
  - ENH-10

duration: 5min
completed: 2026-04-11
---

# Phase 10 Plan 1: Agregar datos extra en timecards Summary

**ExtraInfo infrastructure on TimerModel with cycle number, FCT%, and dose counts wired through MainViewModel — subtle 12pt #999999 labels without card height increase**

## Performance

- **Duration:** 5 min
- **Started:** 2026-04-11T21:15:00Z
- **Completed:** 2026-04-11T21:20:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Added ExtraInfo (string) and IsExtraInfoVisible (bool) ObservableProperty fields to TimerModel
- Replaced elapsed time Label with HorizontalStackLayout containing elapsed + ExtraInfo Label in TimerCard.xaml
- Added _adrenalinaDoseCount counter to MainViewModel
- Wired cycle number display on NewCycle, FCT% calculation on compressions elapsed/running changes, and dose counts on Adrenalina/Amiodarona commands
- ResetCodeState clears all ExtraInfo and resets _adrenalinaDoseCount

## Task Commits

1. **Task 1: Add ExtraInfo binding infrastructure to TimerModel and TimerCard** - `397eb7f` (feat)
2. **Task 2: Wire MainViewModel integration — dose counters, cycle number, FCT calculation** - `5724d6d` (feat)

**Plan metadata:** *(pending)*

## Files Created/Modified
- `AclsTracker/Models/TimerModel.cs` - Added ExtraInfo and IsExtraInfoVisible ObservableProperty fields
- `AclsTracker/Controls/TimerCard.xaml` - Replaced elapsed Label with HorizontalStackLayout (elapsed + ExtraInfo Label)
- `AclsTracker/ViewModels/MainViewModel.cs` - Added _adrenalinaDoseCount, wired all ExtraInfo integration points, FCT calculation method

## Decisions Made
- Used ObservableProperty on TimerModel for ExtraInfo binding (avoids converters, enables direct XAML binding)
- FCT calculated via PropertyChanged subscriptions on Timers[0].Elapsed and Timers[2].IsRunning (not inside TimerService — keeps service decoupled from VM logic)
- HorizontalStackLayout preserves all existing elapsed time behavior (format, color triggers) while adding extra info to the right
- FCT hidden when compressions paused/stopped (pulse check, code stopped) per visibility rules

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## Next Phase Readiness
- ExtraInfo infrastructure is in place and ready for any additional timer card data
- All 4 timers (Ciclo RCP, T.Comp, Adrenalina, Amiodarona) show appropriate extra data
- Tiempo Total and T. Pulsos correctly show no extra data
- Device testing recommended to verify visual appearance of 12pt #999999 labels on different screen sizes

---

*Phase: 10-agregar-datos-extra-en-timecards-numero-de-ciclo-fraccion-de-compresion-en-t-comp-numero-de-dosis-en-adrenalina-y-amiodarona*
*Completed: 2026-04-11*

## Self-Check: PASSED

- TimerModel.cs: FOUND
- TimerCard.xaml: FOUND
- MainViewModel.cs: FOUND
- Commit 397eb7f: FOUND
- Commit 5724d6d: FOUND
