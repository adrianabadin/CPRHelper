---
phase: 03
slug: protocol-guidance
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-29
---

# Phase 03 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — manual testing only (DisplayAlert UI popups) |
| **Config file** | none |
| **Quick run command** | `dotnet build AclsTracker/AclsTracker.csproj` |
| **Full suite command** | `dotnet build AclsTracker/AclsTracker.csproj` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build AclsTracker/AclsTracker.csproj`
- **After every plan wave:** Run `dotnet build AclsTracker/AclsTracker.csproj`
- **Before `/gsd-verify-work`:** Build must succeed, then manual device testing
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 03-01-01 | 01 | 1 | REGI-04 | build | `dotnet build` | ❌ W0 | ⬜ pending |
| 03-01-02 | 01 | 1 | REGI-04 | build | `dotnet build` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- No test framework required — all behaviors are DisplayAlert UI popups requiring on-device verification
- Build verification is sufficient for automated checks
- Manual test plan: Start code → wait 2 min → verify pulse check suggestions → change rhythms → verify popups → verify event log entries

*Existing infrastructure covers all phase requirements for automated verification (build).*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Pulse check popup shows IV/IO on first cycle | REGI-04 | DisplayAlert UI popup | Start code → wait 2 min → verify "¿Colocó acceso IV/IO?" appears → press CONTINUAR → wait 2 min → verify IV/IO does NOT appear |
| Pulse check shows "Rotar compresor" every cycle | REGI-04 | DisplayAlert UI popup | Start code → wait 2 min → verify appears → CONTINUAR → wait 2 min → verify appears again |
| Pulse check shows pending H's and T's with names | REGI-04 | DisplayAlert UI popup | Start code → wait 2 min → verify "Revisar H's y T's pendientes" with item names |
| Rhythm change to AESP shows reversible causes popup | REGI-04 | DisplayAlert UI popup | Select AESP rhythm → verify popup appears |
| Rhythm change to Asistolia shows reversible causes popup | REGI-04 | DisplayAlert UI popup | Select Asistolia rhythm → verify popup appears |
| Rhythm change to TV shows defibrillation + H's and T's popup | REGI-04 | DisplayAlert UI popup | Select TV rhythm → verify new popup replaces old one |
| Rhythm change to FV shows defibrillation + H's and T's popup | REGI-04 | DisplayAlert UI popup | Select FV rhythm → verify new popup replaces old one |
| Rhythm change to RCE shows post-ROSC checklist | REGI-04 | DisplayAlert UI popup | Select RCE rhythm → verify full checklist popup |
| ACEPTAR logs to event log | REGI-04 | Event log verification | Press ACEPTAR on any rhythm popup → check event log contains "Recomendación aceptada" |
| RECHAZAR logs to event log | REGI-04 | Event log verification | Press RECHAZAR on any rhythm popup → check event log contains "Recomendación rechazada" |
| Popup stacking prevented on rapid rhythm changes | REGI-04 | Rapid interaction test | Quickly tap 3 different rhythms → verify no popup queue buildup |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify (build) or Wave 0 dependencies documented
- [x] Sampling continuity: build verification covers all tasks
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter (set after plans created)

**Approval:** pending
