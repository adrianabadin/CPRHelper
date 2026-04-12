---
phase: 11
slug: ui-ux-redesign-new-color-palette-tooltips-and-improved-navigability-without-changing-layout-dimensions
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-12
---

# Phase 11 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual visual verification (no automated test project exists) |
| **Config file** | none |
| **Quick run command** | `grep -rn '#[0-9A-Fa-f]\{6\}' AclsTracker/**/*.xaml` |
| **Full suite command** | Deploy to emulator and run manual checklist below |
| **Estimated runtime** | ~120 seconds (manual) |

---

## Sampling Rate

- **After every task commit:** Run quick grep for inline hex colors (regression check)
- **After every plan wave:** Deploy to emulator and verify visual checklist
- **Before `/gsd:verify-work`:** Full manual checklist must pass
- **Max feedback latency:** ~120 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 11-01-xx | 01 | 1 | Color centralization | grep | `grep -rn '#[0-9A-Fa-f]' AclsTracker/**/*.xaml` | ✅ | ⬜ pending |
| 11-02-xx | 02 | 1 | Tooltip system | manual | Emulator: tap each ⓘ icon | N/A | ⬜ pending |
| 11-03-xx | 03 | 2 | Tab bar toggle | manual | Emulator: toggle tab bar | N/A | ⬜ pending |
| 11-04-xx | 04 | 2 | EventLog removal | manual+grep | `grep -rn 'EventLogPanel' AclsTracker/Views/MainPage.xaml` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers all phase requirements. No automated test framework needed — this is pure UI/styling work with manual visual verification.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Colors render from ResourceDictionary | Color centralization | Visual styling, no logic | Inspect XAML for inline hex; deploy to emulator |
| Tooltips appear on ⓘ tap and auto-dismiss in 3-4s | Tooltip system | UI interaction timing | Tap each ⓘ icon, verify popup + auto-dismiss |
| Tooltips dismissable by tap | Tooltip system | UI interaction | Tap tooltip backdrop while visible |
| Tab bar hides on MainPage load | Navigability | Shell behavior | Launch app, verify no tab bar visible |
| Tab bar toggle reveals/hides tabs | Navigability | Gesture interaction | Tap toggle button, verify tab bar appears/disappears |
| EventLogPanel absent from MainPage | EventLog removal | Visual layout check | Scroll MainPage, verify no event feed |
| Timers continue during tooltip display | Non-disruption | Runtime state | Start code, show tooltip, verify timers keep running |
| Dark mode colors render correctly | Color system | Visual check | Switch OS theme to dark, verify all semantic colors |

---

## Validation Sign-Off

- [ ] All tasks have manual verify checklist or grep command
- [ ] Sampling continuity: grep check after every commit, emulator after every wave
- [ ] Wave 0: no automated framework needed (UI-only phase)
- [ ] No watch-mode flags
- [ ] Feedback latency < 120s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
