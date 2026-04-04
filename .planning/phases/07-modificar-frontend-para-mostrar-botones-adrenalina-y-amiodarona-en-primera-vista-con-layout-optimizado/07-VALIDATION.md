---
phase: 7
slug: modificar-frontend-para-mostrar-botones-adrenalina-y-amiodarona-en-primera-vista-con-layout-optimizado
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-04
---

# Phase 7 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — no test project in solution |
| **Config file** | none |
| **Quick run command** | Manual: deploy to Android emulator, verify no scroll needed |
| **Full suite command** | Manual: visual inspection on 360dp-width device/emulator |
| **Estimated runtime** | ~60 seconds (build + deploy + visual check) |

---

## Sampling Rate

- **After every task commit:** Deploy to emulator, visual check
- **After every plan wave:** Full visual inspection of all 7 behaviors
- **Before `/gsd:verify-work`:** All manual-smoke and manual-functional checks must pass
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 07-01-01 | 01 | 1 | Rhythm single row | manual-smoke | Visual: 5 buttons in 1 row | N/A | ⬜ pending |
| 07-01-02 | 01 | 1 | Action buttons 2x2 | manual-smoke | Visual: NUEVO CICLO + DEFIBRILAR side by side | N/A | ⬜ pending |
| 07-01-03 | 01 | 1 | Drug buttons visible | manual-smoke | Visual: ADRENALINA + AMIODARONA without scroll | N/A | ⬜ pending |
| 07-01-04 | 01 | 1 | MetronomePulse smaller | manual-smoke | Visual: circle animates at 45px | N/A | ⬜ pending |
| 07-01-05 | 01 | 1 | TimerCard compact | manual-smoke | Visual: timer font 20px, padding 4 | N/A | ⬜ pending |
| 07-01-06 | 01 | 1 | DataTriggers work | manual-functional | Trigger drug suggestion → color changes | N/A | ⬜ pending |
| 07-01-07 | 01 | 1 | BoolToOpacity works | manual-functional | AMIODARONA dimmed before first adrenalina | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers all phase requirements. No test framework setup needed — all verification is visual/manual on Android emulator.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| ADRENALINA visible without scroll | Layout optimization | Visual layout check, no automated UI test framework | Deploy to 360dp Android emulator, verify both drug buttons fully visible without scrolling |
| Rhythm selector single row | Layout compression | Visual check | Verify all 5 buttons render in single horizontal row, no wrapping |
| ASISTOLIA text not clipped | Text fit | Visual check | Verify "ASISTOLIA" renders fully, no ellipsis or clipping |
| DataTriggers functional | Existing behavior | Interactive test | Start code, trigger adrenalina suggestion, verify button turns red |
| MetronomePulse animation | Existing behavior | Animation visual | Start metronome, verify pulse circle animates smoothly at 45px |

---

## Validation Sign-Off

- [ ] All tasks have manual verify instructions
- [ ] Sampling continuity: visual check after every task commit
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
