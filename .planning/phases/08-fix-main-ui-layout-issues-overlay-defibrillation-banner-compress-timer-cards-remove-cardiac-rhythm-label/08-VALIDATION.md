---
phase: 8
slug: fix-main-ui-layout-issues-overlay-defibrillation-banner-compress-timer-cards-remove-cardiac-rhythm-label
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-05
---

# Phase 8 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — .NET MAUI project with no unit test project |
| **Config file** | none |
| **Quick run command** | `dotnet build AclsTracker/AclsTracker.csproj` |
| **Full suite command** | Manual device/emulator testing |
| **Estimated runtime** | ~30 seconds (build) |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build AclsTracker/AclsTracker.csproj`
- **After every plan wave:** Build passes + manual visual check on Android emulator
- **Before `/gsd:verify-work`:** All 5 manual UI checks pass on Android emulator
- **Max feedback latency:** 30 seconds (build time)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 08-01-01 | 01 | 1 | UI-08-A | build | `dotnet build AclsTracker/AclsTracker.csproj` | N/A | pending |
| 08-01-02 | 01 | 1 | UI-08-B,C | build | `dotnet build AclsTracker/AclsTracker.csproj` | N/A | pending |
| 08-01-03 | 01 | 1 | UI-08-D,E | build | `dotnet build AclsTracker/AclsTracker.csproj` | N/A | pending |
| 08-01-VER | 01 | 1 | ALL | manual | N/A | N/A | pending |

*Status: pending / green / red / flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — no test framework to install. Build verification is the only automated check.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Banner overlays content, no layout space consumed | UI-08-A | Visual layout — no automated UI test framework | Trigger defibrillation banner, verify content doesn't shift down |
| TimerCard uniform 3-row height across all 6 cards | UI-08-B | Visual measurement — requires device rendering | Compare all 6 timer cards for identical heights |
| "T.Comp" appears as compressions timer name | UI-08-C | Text label on device | Start code, verify timer [2] shows "T.Comp" |
| Active rhythm button shows white border | UI-08-D | Visual styling on device | Select each rhythm, verify white border appears on active button |
| "RITMO CARDÍACO" header removed | UI-08-E | Visual absence check | Verify no "RITMO CARDÍACO" header text above rhythm buttons |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
