---
phase: 03-protocol-guidance
verified: 2026-03-29T12:30:00Z
status: human_needed
score: 10/10 must-haves verified
human_verification:
  - test: "Change rhythm to AESP then Asistolia — verify popup shows with ACEPTAR/RECHAZAR buttons"
    expected: "Popup titled 'Protocolo ACLS' with 'Buscar causas reversibles' message and two buttons"
    why_human: "DisplayAlert popup rendering requires on-device/emulator testing"
  - test: "Change rhythm to TV then FV — verify defibrillation popup replaces old one-button popup"
    expected: "Popup titled 'Ritmo Desfibrilable' with ACEPTAR/RECHAZAR (not single ACEPTAR button)"
    why_human: "Visual confirmation of button count requires on-device testing"
  - test: "Change rhythm to RCE — verify post-ROSC checklist popup"
    expected: "Popup with post-ROSC checklist (airway, monitoring, ECG, temp, reversible causes)"
    why_human: "Multi-line popup rendering and content requires visual verification"
  - test: "Press ACEPTAR on a rhythm popup — check event log for decision entry"
    expected: "Event log shows 'Recomendación aceptada: {first line of message}'"
    why_human: "Event log display requires running app to verify"
  - test: "Press RECHAZAR on a rhythm popup — check event log for decision entry"
    expected: "Event log shows 'Recomendación rechazada: {first line of message}'"
    why_human: "Event log display requires running app to verify"
  - test: "Rapidly change rhythm TV -> FV -> TV — verify only ONE popup shows"
    expected: "No popup stacking; second/third changes suppressed by _isPopupShowing guard"
    why_human: "Rapid interaction timing requires on-device testing"
  - test: "Start code, wait 2 min for first pulse check — verify IV/IO suggestion appears"
    expected: "Pulse check popup includes '¿Colocó acceso IV/IO?' on first cycle only"
    why_human: "Timer-based popup requires running app; _cycleCount==0 logic needs runtime verification"
  - test: "Complete first cycle, wait for second pulse check — verify IV/IO does NOT appear"
    expected: "Second pulse check shows Rotar compresor but NOT IV/IO"
    why_human: "Cycle counter state behavior requires runtime verification"
  - test: "Leave some H's/T's unchecked, trigger pulse check — verify pending items listed by name"
    expected: "Popup shows 'Revisar H's y T's pendientes: Hipovolemia, Hipoxia, ...'"
    why_human: "Dynamic content generation from HsAndTsItems requires runtime verification"
---

# Phase 3: Protocol Guidance Verification Report

**Phase Goal:** Users receive context-aware reminders based on AHA ACLS 2020 protocol during resuscitation — rhythm-specific popups with ACEPTAR/RECHAZAR decision logging, plus protocol checklist suggestions (IV/IO, compressor rotation, H's and T's) during 2-minute pulse checks
**Verified:** 2026-03-29T12:30:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Changing rhythm to AESP or Asistolia shows reversible causes popup with ACEPTAR/RECHAZAR buttons | ✓ VERIFIED | Lines 93-94: switch cases for AESP/Asistolia → "Protocolo ACLS" / "Buscar causas reversibles". Line 104: DisplayAlert with "ACEPTAR", "RECHAZAR" |
| 2 | Changing rhythm to TV or FV shows defibrillation + H's and T's popup (replaces old popup) | ✓ VERIFIED | Lines 95-96: switch cases → "Ritmo Desfibrilable" / "Preparar desfibrilador.\nConsidere causas reversibles". Grep confirms old "Defibrile y reanude compresiones" NOT present |
| 3 | Changing rhythm to RCE shows post-ROSC checklist popup with ACEPTAR/RECHAZAR | ✓ VERIFIED | Line 97: switch case → "RCE Alcanzado" with full post-ROSC checklist (airway, monitoring, ECG, temp, causes). Same DisplayAlert with ACEPTAR/RECHAZAR |
| 4 | ACEPTAR logs 'Recomendación aceptada: {summary}' to event log | ✓ VERIFIED | Lines 106-108: `decision = accepted ? "aceptada" : "rechazada"`, summary = first line of message, LogCustomEventCommand.Execute($"Recomendación {decision}: {summary}") |
| 5 | RECHAZAR logs 'Recomendación rechazada: {summary}' to event log | ✓ VERIFIED | Same code path — when accepted=false, decision="rechazada" |
| 6 | First pulse check popup includes '¿Colocó acceso IV/IO?' suggestion | ✓ VERIFIED | Lines 159-163: `if (_cycleCount == 0) { suggestions.Add("¿Colocó acceso IV/IO?"); }` |
| 7 | Every pulse check popup includes '¿Rotar compresor?' suggestion | ✓ VERIFIED | Line 166: `suggestions.Add("¿Rotar compresor?");` — unconditional, always added |
| 8 | Pulse check popup shows pending H's and T's by name when any remain unchecked | ✓ VERIFIED | Lines 169-176: LINQ `.Where(!IsChecked && !IsDismissed).Select(i => i.Name).ToList()`, then `$"Revisar H's y T's pendientes: {string.Join(", ", pendingHsTs)}"` |
| 9 | Rapid rhythm changes do not stack multiple popups (guard flag prevents it) | ✓ VERIFIED | Line 87: `if (_isPopupShowing) return;` guard. Line 88: `_isPopupShowing = true;`. Lines 111-114: `finally { _isPopupShowing = false; }` |
| 10 | Cycle counter resets on StartCode, increments on NewCycle | ✓ VERIFIED | Line 50: `_cycleCount = 0;` in StartCode(). Line 120: `_cycleCount++;` in NewCycle() |

**Score:** 10/10 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `AclsTracker/ViewModels/MainViewModel.cs` | Protocol guidance: rhythm change popups + pulse check suggestions + cycle counter | ✓ VERIFIED | 243 lines. Contains HandleRhythmChangeAsync (L85), cycle counter fields (L22-23), protocol suggestions in OnPulseCheckDue (L157-176) |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| PropertyChanged handler (L35) | HandleRhythmChangeAsync (L85) | Fire-and-forget `_ = HandleRhythmChangeAsync(rhythm)` | ✓ WIRED | Line 41: fire-and-forget call when CurrentRhythm changes |
| HandleRhythmChangeAsync (L85) | LogCustomEventCommand | Logs accept/reject decision after DisplayAlert | ✓ WIRED | Line 108: `LogCustomEventCommand.Execute($"Recomendación {decision}: {summary}")` — pattern matches "Recomendación (aceptada\|rechazada)" |
| OnPulseCheckDue suggestion builder (L169) | EventRecording.HsAndTsItems | LINQ filter `.Where(!IsChecked && !IsDismissed)` | ✓ WIRED | Lines 169-172: filters HsAndTsItems, selects Name, materializes with .ToList() |
| StartCode (L47) | _cycleCount | Reset to 0 | ✓ WIRED | Line 50: `_cycleCount = 0;` |
| NewCycle (L118) | _cycleCount | Increment | ✓ WIRED | Line 120: `_cycleCount++;` |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| REGI-04 | 03-01-PLAN | El sistema genera recordatorios contextuales basados en el protocolo ACLS 2020 | ✓ SATISFIED | Rhythm-specific popups (AESP/Asistolia→reversible causes, TV/FV→defibrillation, RCE→post-ROSC) + pulse check suggestions (IV/IO, compressor rotation, H's/T's) provide context-aware protocol reminders |

No orphaned requirements found. REGI-04 is the only requirement mapped to Phase 3 in both PLAN frontmatter and REQUIREMENTS.md traceability table.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| (none) | — | — | — | No anti-patterns detected |

No TODO/FIXME/PLACEHOLDER comments. No empty implementations. No stub return values. No console.log-only handlers.

### Build Verification

**Result:** ✓ PASSED — 0 errors
- Warnings are pre-existing (Application.MainPage deprecation CS0618, XamlC compiled binding suggestions) — not introduced by this phase
- All 4 target frameworks build successfully (android, ios, maccatalyst, windows)

### Commit Verification

| Commit | Task | Status |
|--------|------|--------|
| `5d25c0e` | Task 1: HandleRhythmChangeAsync + cycle counter | ✓ Found |
| `284ae76` | Task 2: OnPulseCheckDue protocol suggestions | ✓ Found |

### Human Verification Required

### 1. Rhythm Change Popups — Visual Content

**Test:** Start a code → change rhythm to each of AESP, Asistolia, TV, FV, RCE
**Expected:** Each shows appropriate protocol-specific popup with ACEPTAR/RECHAZAR buttons
**Why human:** DisplayAlert rendering and visual content requires on-device/emulator testing

### 2. ACEPTAR/RECHAZAR Decision Logging

**Test:** Press ACEPTAR on one rhythm popup, RECHAZAR on another → check event log
**Expected:** Event log shows "Recomendación aceptada: Buscar causas reversibles" and "Recomendación rechazada: Ritmo desfibrilable. Preparar desfibrilador."
**Why human:** Event log display and timestamp formatting requires running app

### 3. Popup Stacking Guard

**Test:** Rapidly tap rhythm changes (TV → FV → TV → AESP) within <1 second
**Expected:** Only one popup shows; subsequent changes are suppressed while popup is visible
**Why human:** Rapid interaction timing and _isPopupShowing guard behavior requires runtime testing

### 4. First-Cycle IV/IO Reminder

**Test:** Start code → wait 2 min → observe first pulse check popup
**Expected:** Popup includes "¿Colocó acceso IV/IO?" (only on first cycle)
**Why human:** Timer-triggered popup content and _cycleCount==0 logic requires runtime verification

### 5. IV/IO Absence on Second Cycle

**Test:** Complete first cycle (NewCycle) → wait 2 min → observe second pulse check popup
**Expected:** "¿Colocó acceso IV/IO?" does NOT appear; only "¿Rotar compresor?" and any pending H's/T's
**Why human:** Cycle counter state transition requires runtime verification

### 6. Pending H's and T's Listed by Name

**Test:** Leave several H's/T's items unchecked → trigger pulse check
**Expected:** Popup shows "Revisar H's y T's pendientes: Hipovolemia, Hipoxia, ..." with actual unchecked item names
**Why human:** Dynamic content generation from ObservableCollection requires runtime verification

### 7. Old Popup Fully Replaced

**Test:** Change rhythm to TV — verify popup has TWO buttons (ACEPTAR/RECHAZAR), not one
**Expected:** Two-button popup with "Considere causas reversibles (H's y T's)" — NOT old "Defibrile y reanude compresiones" single-button
**Why human:** Visual confirmation of button count requires on-device testing

### Gaps Summary

No code-level gaps found. All 10 must-have truths verified against the actual codebase. All artifacts exist, are substantive (243-line file with full implementations), and are properly wired. Build succeeds with 0 errors. No anti-patterns detected.

The phase requires on-device human testing to confirm that UI popups render correctly, the _isPopupShowing guard works under rapid interaction, cycle counter state transitions are correct across multiple cycles, and the dynamic H's/T's suggestion content generates properly.

---

_Verified: 2026-03-29T12:30:00Z_
_Verifier: Claude (gsd-verifier)_
