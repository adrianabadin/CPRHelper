---
phase: 04
slug: data-persistance
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-29
---

# Phase 04 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — MAUI project, manual testing on emulator |
| **Config file** | None |
| **Quick run command** | `dotnet build AclsTracker --no-restore` |
| **Full suite command** | `dotnet build AclsTracker` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build AclsTracker --no-restore`
- **After every plan wave:** Run `dotnet build AclsTracker`
- **Before `/gsd-verify-work`:** Full build must succeed
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 04-01-01 | 01 | 1 | DB models + mapper | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |
| 04-01-02 | 01 | 1 | Repository + DI | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |
| 04-02-01 | 02 | 2 | Patient popup UI | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |
| 04-02-02 | 02 | 2 | Save flow in StopCode | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |
| 04-03-01 | 03 | 2 | HistorialViewModel + search | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |
| 04-03-02 | 03 | 2 | HistorialPage XAML evolution | build | `dotnet build AclsTracker --no-restore 2>&1 \| tail -5` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [x] Build infrastructure already exists (`dotnet build`)
- [x] No test framework needed — MAUI UI project, build verification sufficient

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Patient data popup appears on FINALIZAR CÓDIGO | Save flow | MAUI DisplayAlert/Popup requires runtime | Start code → Stop code → Verify popup shows nombre/apellido/DNI fields |
| Session saved to SQLite with all events | Persistence | DB file inspection requires runtime | Save session → Restart app → Verify session in HistorialPage |
| Search by patient name returns correct sessions | Search | SQLite query + UI binding requires runtime | Save 2+ sessions → Search by name → Verify results |
| Search by date range works | Search | Date picker + query requires runtime | Save sessions on different days → Filter by date → Verify results |
| Session detail shows all events in order | Detail view | CollectionView binding requires runtime | Tap saved session → Verify all events appear chronologically |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify (build check)
- [x] Sampling continuity: every task has build verify
- [x] Wave 0 covers all MISSING references (none — build infra exists)
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter (after execution)

**Approval:** pending
