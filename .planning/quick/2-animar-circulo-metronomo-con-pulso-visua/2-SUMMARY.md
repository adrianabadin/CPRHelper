---
phase: quick
plan: 2
subsystem: metronome-animation
tags: [animation, metronome, visual-feedback, heartbeat, ux]
dependency_graph:
  requires: []
  provides: [heartbeat-pulse-animation]
  affects: [MetronomePulse, MetronomeViewModel]
tech_stack:
  added: []
  patterns: [Task.WhenAll parallel animations, CubicOut/CubicIn easing for natural feel]
key_files:
  created: []
  modified:
    - AclsTracker/Controls/MetronomePulse.xaml
    - AclsTracker/Controls/MetronomePulse.xaml.cs
decisions:
  - "Parallel Task.WhenAll for scale+opacity on pump peaks — avoids sequential await overhead"
  - "CancelAnimations() at start of each beat — prevents stuttering when beats overlap at high BPM"
  - "330ms total fits 500ms interval at 120 BPM with 170ms safety margin"
metrics:
  duration: "~3 minutes"
  completed_date: "2026-04-04"
  tasks_completed: 1
  files_modified: 2
---

# Quick Task 2: Heartbeat Pulse Animation on MetronomePulse Circle Summary

**One-liner:** Replaced single-scale metronome pulse with lub-dub double-pump heartbeat animation (1.35x + 1.25x scale with parallel opacity) totaling 330ms synchronized to each audio beat.

## What Was Done

Upgraded the `MetronomePulse` control's `AnimatePulse` method from a simple single-expansion animation (1.0 -> 1.15 -> 1.0 in 200ms) to a realistic heartbeat lub-dub double-pump pattern:

- **First pump (lub/systole):** Scale to 1.35x + FadeTo 1.0 over 80ms with CubicOut easing (parallel)
- **Partial relax:** Scale back to 1.1x over 60ms with CubicIn easing
- **Second pump (dub/diastole):** Scale to 1.25x over 70ms with CubicOut easing
- **Full relax:** Scale back to 1.0x + FadeTo 0.8 over 120ms (parallel)

Total animation duration: ~330ms, comfortably within the 500ms beat interval at 120 BPM.

Also increased `PulseCircle` size from 40x40 to 42x42 for better visibility of the animation range. The Grid container at 45x45 provides adequate layout space since scale animations use transforms (not layout).

## Tasks Completed

| Task | Description | Commit |
|------|-------------|--------|
| 1 | Implement heartbeat double-pump animation | c940788 |

## Deviations from Plan

None — plan executed exactly as written.

## Self-Check

- [x] `AclsTracker/Controls/MetronomePulse.xaml.cs` — modified with double-pump AnimatePulse
- [x] `AclsTracker/Controls/MetronomePulse.xaml` — PulseCircle size updated to 42x42
- [x] Build passes: 0 errors, 269 pre-existing warnings (unrelated to this change)
- [x] Commit c940788 exists on main branch
- [x] Animation total: 80 + 60 + 70 + 120 = 330ms < 450ms safety threshold

## Self-Check: PASSED
