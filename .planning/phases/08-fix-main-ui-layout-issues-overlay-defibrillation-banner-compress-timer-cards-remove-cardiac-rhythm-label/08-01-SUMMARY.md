---
phase: 08-fix-main-ui-layout-issues-overlay-defibrillation-banner-compress-timer-cards-remove-cardiac-rhythm-label
plan: 01
subsystem: ui
tags: [xaml, maui, animation, layout, overlay]

# Dependency graph
requires:
  - phase: 07-modificar-frontend-para-mostrar-botones-adrenalina-y-amiodarona-en-primera-vista-con-layout-optimizado
    provides: Optimized main page layout with timer cards, rhythm selector, and notification banner
provides:
  - Overlay banner that floats above content (no layout space consumed)
  - Compressed 3-row TimerCard (uniform height for all 6 cards)
  - T.Comp name for compressions timer
  - Active rhythm button white border highlight via DataTrigger
  - Removed RITMO CARDIACO header label
affects: [main-page-ui, timer-cards, rhythm-selector, notification-banner]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Grid overlay pattern: Banner and ScrollView in same Grid cell, ZIndex for layering"
    - "Slide-down/fade-in animation with TranslateTo + FadeTo on Show/HideBanner"
    - "DataTrigger for active state highlighting on rhythm buttons"
    - "Per-property computed bools for DataTrigger binding (IsRhythmX pattern)"

key-files:
  created: []
  modified:
    - AclsTracker/Views/MainPage.xaml
    - AclsTracker/Controls/NotificationBanner.xaml
    - AclsTracker/Controls/NotificationBanner.xaml.cs
    - AclsTracker/Controls/TimerCard.xaml
    - AclsTracker/ViewModels/TimerViewModel.cs
    - AclsTracker/Controls/RhythmSelector.xaml
    - AclsTracker/ViewModels/EventRecordingViewModel.cs

key-decisions:
  - "Banner placed after ScrollView in document order for MAUI same-cell stacking (later = renders on top)"
  - "Fixed -60px translation offset for banner animation (avoids zero-height on first render)"
  - "Fully opaque banner (Opacity=1.0) per user preference"
  - "Removed EN PAUSA row instead of hiding it, ensuring uniform 3-row height on all cards"
  - "Per-rhythm computed bool properties (IsRhythmX) with DataTrigger instead of converters"

patterns-established:
  - "Overlay pattern: Control with ZIndex=10 and VerticalOptions=Start placed after ScrollView in same Grid cell"
  - "Slide animation: TranslateTo(-60→0) + FadeTo(0→1) on show, reverse on hide"
  - "Active state highlight: DataTrigger with computed bool property + BorderColor/BorderWidth setters"

requirements-completed: [UI-08-A, UI-08-B, UI-08-C, UI-08-D, UI-08-E]

# Metrics
duration: 12min
completed: 2026-04-05
---

# Phase 8 Plan 1: Fix Main UI Layout Summary

**Overlay banner, compressed 3-row timer cards (T.Comp), removed RITMO CARDIACO header, active rhythm white border highlight**

## Performance

- **Duration:** 12 min
- **Started:** 2026-04-05T14:28:05Z
- **Completed:** 2026-04-05T14:40:31Z
- **Tasks:** 2 auto tasks + 1 auto-approved checkpoint
- **Files modified:** 7

## Accomplishments
- Banner now overlays content (no layout shift) with slide-down/fade-in animation
- All 6 timer cards have uniform 3-row height (removed variable EN PAUSA row)
- Compressions timer displays "T.Comp" instead of "Compresiones"
- Active rhythm button shows white 2px border highlight via DataTrigger
- Removed RITMO CARDIACO header label, saving vertical space

## Task Commits

Each task was committed atomically:

1. **Task 1: Banner overlay and TimerCard compression** - `7a299ef` (feat)
2. **Task 2: Rhythm selector header removal and active button highlight** - `d8ee839` (feat)

**Plan metadata:** pending (docs: complete plan)

_Note: Task 3 (checkpoint:human-verify) was auto-approved in auto-mode._

## Files Created/Modified
- `AclsTracker/Views/MainPage.xaml` - Banner moved to overlay position (same Grid cell as ScrollView)
- `AclsTracker/Controls/NotificationBanner.xaml` - Opacity changed to 1.0
- `AclsTracker/Controls/NotificationBanner.xaml.cs` - Added slide-down/fade-in animation on Show, reverse on HideBanner
- `AclsTracker/Controls/TimerCard.xaml` - Compressed from 4-row to 3-row Grid, removed EN PAUSA label, moved pause button
- `AclsTracker/ViewModels/TimerViewModel.cs` - Renamed compressions timer from "Compresiones" to "T.Comp"
- `AclsTracker/Controls/RhythmSelector.xaml` - Removed RITMO CARDIACO header, added DataTrigger highlight on all 5 rhythm buttons
- `AclsTracker/ViewModels/EventRecordingViewModel.cs` - Added IsRhythmX computed properties with change notifications

## Decisions Made
- Banner placed after ScrollView in document order for MAUI same-cell stacking (later = renders on top)
- Fixed -60px translation offset for banner animation (avoids zero-height on first render)
- Fully opaque banner (Opacity=1.0) per user preference
- Removed EN PAUSA row entirely instead of hiding it, ensuring uniform 3-row height
- Per-rhythm computed bool properties (IsRhythmX) with DataTrigger for active state highlighting

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All UI layout fixes complete and building successfully
- Vertical space recovered: banner overlay (~50dp), EN PAUSA removal (~20dp), header removal (~20dp)
- ADRENALINA and AMIODARONA buttons should now be visible without scrolling

---
*Phase: 08-fix-main-ui-layout-issues-overlay-defibrillation-banner-compress-timer-cards-remove-cardiac-rhythm-label*
*Completed: 2026-04-05*

## Self-Check: PASSED
- All 7 modified files verified on disk
- 2 task commits found in git log (7a299ef, d8ee839)
- Build passes with 0 errors
