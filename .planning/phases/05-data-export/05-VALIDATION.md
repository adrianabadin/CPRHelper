---
phase: 5
slug: data-export
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-03-30
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — .NET MAUI project, manual testing |
| **Config file** | none |
| **Quick run command** | `dotnet build AclsTracker/AclsTracker.csproj` |
| **Full suite command** | `dotnet build AclsTracker/AclsTracker.csproj` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** `dotnet build AclsTracker/AclsTracker.csproj`
- **After every plan wave:** Build + manual launch test on Windows
- **Before `/gsd-verify-work`:** Build must succeed, manual test both PDF and CSV export
- **Max feedback latency:** 15 seconds (build time)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 05-01-01 | 01 | 1 | EXPO-01 | build | `dotnet build AclsTracker/AclsTracker.csproj` | ✅ | ⬜ pending |
| 05-01-02 | 01 | 1 | EXPO-01 | build | `dotnet build AclsTracker/AclsTracker.csproj` | ✅ | ⬜ pending |
| 05-02-01 | 02 | 1 | EXPO-02 | build | `dotnet build AclsTracker/AclsTracker.csproj` | ✅ | ⬜ pending |
| 05-02-02 | 02 | 1 | EXPO-01, EXPO-02 | build | `dotnet build AclsTracker/AclsTracker.csproj` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements. Phase 5 is UI + file generation — build verification + manual testing is appropriate.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| PDF generates with all 6 sections (header, rhythm, meds, HsTs, compressions, footer) | EXPO-01 | PDF is visual output requiring inspection | Export PDF from saved session detail view, verify all sections present |
| PDF contains correct patient data and formatted timestamps | EXPO-01 | Content verification requires visual check | Open PDF, verify patient name, DNI, date, duration match session |
| CSV opens correctly in Excel with Spanish characters (á, é, ñ) | EXPO-02 | Requires Excel to verify BOM encoding | Export CSV, open in Excel, verify accented characters display correctly |
| Share sheet opens with generated file on both platforms | EXPO-01, EXPO-02 | Platform-specific UI behavior | Tap EXPORTAR PDF/CSV, verify share sheet appears with file |
| Local copy saved to app data directory | EXPO-01, EXPO-02 | File system verification | After export, check AppDataDirectory/Exports/ for file |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references (none needed)
- [x] No watch-mode flags
- [x] Feedback latency < 15s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
