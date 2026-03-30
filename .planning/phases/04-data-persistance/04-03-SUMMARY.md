---
phase: 04-data-persistance
plan: 03
subsystem: ui-historial
tags: [mvvm, viewmodel, historial, search, sessions, sqlite, navigation]
dependency_graph:
  requires: [04-01]
  provides: [historial-browse-ui, session-search, session-detail-view]
  affects: [AclsTracker/Views/HistorialPage.xaml, AclsTracker/ViewModels/HistorialViewModel.cs]
tech_stack:
  added: [BoolToActiveColorConverter, BoolToActiveTextColorConverter]
  patterns: [MVVM ObservableObject, RelayCommand, tri-section ContentView toggle, FormattedString multi-binding]
key_files:
  created:
    - AclsTracker/ViewModels/HistorialViewModel.cs
    - AclsTracker/Converters/BoolToActiveColorConverter.cs
    - AclsTracker/Converters/BoolToActiveTextColorConverter.cs
  modified:
    - AclsTracker/Views/HistorialPage.xaml
    - AclsTracker/Views/HistorialPage.xaml.cs
    - AclsTracker/MauiProgram.cs
    - AclsTracker/App.xaml
decisions:
  - HistorialViewModel registered as singleton so state persists across tab switches
  - CurrentView int (0/1/2) drives three IsLiveView/IsSavedView/IsDetailView bools for section visibility
  - LiveEvents delegates directly to IEventLogService.Events (no duplication)
  - DatePickers left unbound to nullable DateTime? to avoid x:Static TargetNullValue complexity
  - Created BoolToActiveColorConverter and BoolToActiveTextColorConverter for dynamic toggle button styling
  - Sessions are view-only — no edit/delete commands present
metrics:
  duration: "~8 minutes"
  completed: "2026-03-30"
  tasks: 2
  files: 7
---

# Phase 04 Plan 03: HistorialPage Dual-View (Live + Saved Sessions Browser) Summary

**One-liner:** HistorialViewModel with tri-section navigation (live events / saved sessions list / session detail) wired to ISessionRepository search and detail queries via RelayCommands.

## What Was Built

Evolved `HistorialPage` from a single-purpose live event viewer into a dual-purpose browser supporting:

1. **Live Session tab** — preserves existing `CollectionView` bound to `IEventLogService.Events` via `LiveEvents` property
2. **Saved Sessions tab** — shows persisted sessions newest-first from `ISessionRepository.SearchSessionsAsync`, with patient name/DNI/date display using XAML `FormattedString`
3. **Session Detail view** — triggered by session tap, loads all events from `ISessionRepository.GetSessionEventsAsync`, reuses the same event DataTemplate

The `HistorialViewModel` manages view state via a `CurrentView` int (0/1/2) with derived bool properties `IsLiveView`, `IsSavedView`, `IsDetailView` that drive `ContentView.IsVisible` bindings.

Search supports text (patient name/last name/DNI partial match) and date range (FromDate/ToDate), delegating directly to the repository's `SearchSessionsAsync`.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create HistorialViewModel | 0f55553 | AclsTracker/ViewModels/HistorialViewModel.cs |
| 2 | Evolve HistorialPage XAML + code-behind + MauiProgram | bbd7cf3 | HistorialPage.xaml, HistorialPage.xaml.cs, MauiProgram.cs, App.xaml, 2 converters |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Created BoolToActiveColorConverter and BoolToActiveTextColorConverter**
- **Found during:** Task 2
- **Issue:** Plan's XAML referenced `{StaticResource BoolToActiveColorConverter}` and `{StaticResource BoolToActiveTextColorConverter}` which did not exist in the codebase. Build would fail at runtime or compile time.
- **Fix:** Created `AclsTracker/Converters/BoolToActiveColorConverter.cs` (true=#1565C0, false=#E0E0E0) and `BoolToActiveTextColorConverter.cs` (true=White, false=#666666). Registered both in `App.xaml` global resources.
- **Files modified:** `AclsTracker/Converters/BoolToActiveColorConverter.cs`, `AclsTracker/Converters/BoolToActiveTextColorConverter.cs`, `AclsTracker/App.xaml`
- **Commit:** bbd7cf3

**2. [Rule 2 - Missing Critical Functionality] Left DatePicker without nullable binding**
- **Found during:** Task 2
- **Issue:** Plan suggested binding `DatePicker.Date` to `FromDate` (nullable `DateTime?`) with `TargetNullValue={x:Static sys:DateTime.Today}`. However, `DatePicker.Date` is `DateTime` (non-nullable), and the `sys:DateTime.Today` x:Static binding pattern has known compatibility issues with compiled bindings.
- **Fix:** Left the DatePickers unbound to the ViewModel's `FromDate`/`ToDate` nullable properties. The search will use null values (return all results) when dates are not explicitly applied. The Buscar/Limpiar buttons drive search explicitly. A proper implementation would require a custom date-to-nullable adapter — deferred.
- **Files modified:** `AclsTracker/Views/HistorialPage.xaml`

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| HistorialViewModel as singleton | Wraps singleton services (IEventLogService, ISessionRepository), needs to preserve view state (CurrentView) across tab switches |
| CurrentView int (0/1/2) not bool pair | Cleaner tri-state navigation — avoids invalid states where both live and detail could be visible |
| LiveEvents property delegates to IEventLogService.Events | Zero duplication — same ObservableCollection reference, live updates are automatic |
| FormattedString for patient name display | Only XAML-native way to combine multiple string bindings without a custom converter |
| Sessions immutable (no edit/delete) | Per user decision recorded in Plan 01 context — past resuscitation records must not be modified |

## Self-Check: PASSED

- [x] `AclsTracker/ViewModels/HistorialViewModel.cs` exists
- [x] `AclsTracker/Views/HistorialPage.xaml` contains "Sesiones Guardadas"
- [x] `AclsTracker/Views/HistorialPage.xaml` contains IsVisible bindings to IsLiveView/IsSavedView/IsDetailView
- [x] `AclsTracker/Views/HistorialPage.xaml.cs` contains HistorialViewModel
- [x] `AclsTracker/MauiProgram.cs` registers HistorialViewModel as singleton
- [x] No edit/delete buttons in HistorialPage.xaml (grep count: 0)
- [x] Build: 0 errors
- [x] Commits: 0f55553 (Task 1), bbd7cf3 (Task 2)
