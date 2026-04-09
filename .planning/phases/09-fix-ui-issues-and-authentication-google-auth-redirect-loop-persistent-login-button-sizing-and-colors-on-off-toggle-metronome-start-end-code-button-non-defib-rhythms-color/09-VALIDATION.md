---
phase: 9
slug: fix-ui-issues-and-authentication-google-auth-redirect-loop-persistent-login-button-sizing-and-colors-on-off-toggle-metronome-start-end-code-button-non-defib-rhythms-color
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-09
---

# Phase 9 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — no test project exists; bug-fix phase only |
| **Config file** | none |
| **Quick run command** | `dotnet build AclsTracker/AclsTracker.csproj -c Debug -f net8.0-android` |
| **Full suite command** | `dotnet build AclsTracker/AclsTracker.csproj -c Debug` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build AclsTracker/AclsTracker.csproj -c Debug -f net8.0-android`
- **After every plan wave:** Run `dotnet build AclsTracker/AclsTracker.csproj -c Debug`
- **Before `/gsd-verify-work`:** Full build must succeed + human verification on device
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 09-01-01 | 01 | 1 | P09-AUTH-01, P09-AUTH-02 | build + manual | `dotnet build` | N/A | ⬜ pending |
| 09-01-02 | 01 | 1 | P09-UI-01, P09-UI-02 | build + visual | `dotnet build` | N/A | ⬜ pending |
| 09-02-01 | 02 | 1 | P09-UI-03, P09-UI-04 | build + visual | `dotnet build` | N/A | ⬜ pending |
| 09-02-02 | 02 | 1 | ALL | manual | human verify | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements. No test project to add — scope is bug-fix only.*

- [x] Build command available: `dotnet build AclsTracker/AclsTracker.csproj`
- [x] No new test framework needed for this phase

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Google OAuth completes end-to-end without redirect loop | P09-AUTH-01 | Requires real Google account + browser + device | 1. Tap Login → Google → browser opens → complete auth → app returns with avatar visible. Verify NOT redirected to localhost:3000 |
| Session persists across app restart | P09-AUTH-02 | Requires OS-level app restart cycle (SecureStorage is real Keystore/Keychain) | 1. Login with any method 2. Force-close app 3. Reopen → avatar still visible, no re-login needed |
| ON/OFF label renders correctly | P09-UI-01 | Visual XAML rendering | 1. Start code 2. Toggle metronome ON/OFF → verify text shows "ON"/"OFF" not "True"/"False" |
| Metronome row fits without overflow | P09-UI-02 | Visual layout check | 1. Start code 2. Verify metronome controls fit in one row without wrap |
| INICIAR button color and height | P09-UI-03 | Visual check | 1. Verify button is orange (#E65100), height ~40px |
| AESP/Asistolia yellow + dark text | P09-UI-04 | Visual check | 1. Select AESP → yellow bg + dark text. 2. Select Asistolia → same. 3. Verify other rhythms unchanged |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify (build acts as smoke)
- [x] Wave 0 covers all MISSING references (none needed)
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
