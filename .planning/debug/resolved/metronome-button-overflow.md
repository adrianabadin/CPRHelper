---
status: resolved
trigger: "Metronome +/- buttons still don't fit on screen after Phase 09 Plan 2 reduced them to 36x36. Need at least 15px more horizontal space."
created: 2025-04-11T00:00:00Z
updated: 2026-04-09T19:00:00Z
---

## Current Focus

hypothesis: Total horizontal width of HorizontalStackLayout exceeds available screen width due to accumulated element sizes + spacing + padding
test: Calculate total pixel width of all elements and compare to typical Android screen width (~360dp)
expecting: Identify where ≥15px can be freed
next_action: Apply targeted size reductions to XAML

## Symptoms

expected: Entire metronome row (pulse circle + BPM display + -/ON|OFF/+ buttons) fits in one line without overflow
actual: +/- buttons still overflow or get clipped on screen
errors: None — visual overflow
reproduction: Open app → start metronome → observe button row layout
started: Phase 09 Plan 02 reduced buttons from 44→36px but still not enough

## Eliminated

- hypothesis: Button size alone was the problem (44→36px in P09-02)
  evidence: Still overflows at 36px — other elements also consume space

## Evidence

- timestamp: 2025-04-11
  checked: MetronomePulse.xaml full file — HorizontalStackLayout with Spacing=12, Padding="8,4"
  found: 6 children, 5 spacing gaps, outer padding
  implication: Total width = sum(elements) + 5×12 + 16 = needs calculation

- timestamp: 2025-04-11
  checked: Pixel width calculation of current layout
  found:
    Pulse Grid: 45px
    BPM Label (FontSize=30, "200"): ~52px
    "BPM" Label (FontSize=12) + Margin.Right=8: ~36px
    - Button: 36px
    Toggle (ON/OFF): 56px
    + Button: 36px
    Spacing: 5 × 12 = 60px
    Padding: 8 + 8 = 16px
    TOTAL: ~333px
  implication: On 360dp screen leaves only 27dp margin; on narrower screens (320-340dp) overflows

- timestamp: 2025-04-11
  checked: MetronomePulse.xaml.cs — animation code
  found: Uses ScaleTo (multiplicative) — safe to change circle dimensions
  implication: Can reduce pulse circle size without breaking animation

## Resolution

root_cause: HorizontalStackLayout total width ~333px exceeded screen. Toggle 56px + spacing 12 + padding 8 too large.
fix_applied: Commit 74493d5 — toggle 56→48, +/- buttons 36→32, spacing 12→6, padding 8→4. Total savings: ~18px horizontal.
verification: Build passed (Windows target). Visual verification needed on device.

### Space budget — proposed savings

| Element | Current | Proposed | Savings |
|---------|---------|----------|---------|
| Spacing (5 gaps) | 5×12=60 | 5×8=40 | 20 |
| Padding (sides) | 8+8=16 | 6+6=12 | 4 |
| Pulse Grid | 45 | 38 | 7 |
| Pulse BoxView | 42 | 35 | 7 |
| CornerRadius | 21 | 17.5 | — |
| BPM Margin.Right | 8 | 4 | 4 |
| **Total savings** | | | **~34** |
| New total width | 333 | ~299 | ✓ fits 360dp |
