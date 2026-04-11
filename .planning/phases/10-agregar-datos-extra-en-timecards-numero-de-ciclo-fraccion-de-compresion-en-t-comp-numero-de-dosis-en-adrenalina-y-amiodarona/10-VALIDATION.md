---
phase: 10
slug: agregar-datos-extra-en-timecards-numero-de-ciclo-fraccion-de-compresion-en-t-comp-numero-de-dosis-en-adrenalina-y-amiodarona
status: draft
nyquist_compliant: false
wave_0_complete: true
created: 2026-04-11
---

# Phase 10 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — .NET MAUI project has no unit test framework |
| **Config file** | none |
| **Quick run command** | `dotnet build AclsTracker/AclsTracker.csproj -f net8.0-android --no-restore` |
| **Full suite command** | Manual device/emulator walkthrough |
| **Estimated runtime** | ~30 seconds (build) + 2 min (manual walkthrough) |

---

## Sampling Rate

- **After every task commit:** `dotnet build` to verify compilation
- **After every plan wave:** Manual walkthrough on emulator (start code → cycle → drugs → FCT → stop → continue → new code)
- **Before `/gsd-verify-work`:** Full walkthrough with all 4 extra info displays verified
- **Max feedback latency:** 30 seconds (build time)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 10-01-01 | 01 | 1 | ENH-10 | build | `dotnet build` | ✅ | ⬜ pending |
| 10-01-02 | 01 | 1 | ENH-10 | build | `dotnet build` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — no test framework needed for this UI enhancement phase.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Cycle number appears in Ciclo RCP TimerCard | ENH-10 | Visual verification on device | Start code → wait for first cycle → verify "1" appears next to time in Ciclo RCP card |
| FCT% appears in T.Comp TimerCard | ENH-10 | Real-time visual verification | Start code → start compressions → verify FCT% updates in real-time next to T.Comp time |
| Adrenalina dose count appears | ENH-10 | Interactive visual verification | Tap ADRENALINA → verify count increments in Adrenalina TimerCard |
| Amiodarona dose count appears | ENH-10 | Interactive visual verification | Tap AMIODARONA → verify count increments in Amiodarona TimerCard |
| ExtraInfo hides when value is 0 | ENH-10 | Visual state verification | Start code → verify no extra info visible → first cycle adds cycle number |
| ExtraInfo clears on NUEVO CODIGO | ENH-10 | Multi-step interaction | Start code → cycle → drugs → stop → NUEVO CODIGO → verify all extra info cleared |
| ExtraInfo persists on CONTINUAR | ENH-10 | Multi-step interaction | Start code → cycle → drugs → stop → CONTINUAR → verify counts preserved |
| Subtle style (12pt gray #999999) | ENH-10 | Visual style verification | Verify extra info is clearly secondary to time display |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify (build check)
- [x] Sampling continuity: every task has build verify
- [x] Wave 0 covers all MISSING references (no test framework needed)
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter (after execution)

**Approval:** pending
