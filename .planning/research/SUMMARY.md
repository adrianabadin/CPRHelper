# Research Summary

**Project:** ACLS Tracker — Mobile Advanced Cardiac Life Support Application
**Researched:** 24/03/2026
**Confidence:** HIGH

## Overview

Research focused on mobile medical application development for ACLS (Advanced Cardiac Life Support) cardiac arrest management, specifically a cross-platform (Android + iOS) metronome and timer app following AHA 2020 guidelines. Research covered technology stack, feature landscape, architecture patterns, and common pitfalls in emergency medical app development.

## Key Findings

### Stack Recommendation

**Recommended Technology:** .NET MAUI (over React Native)

**Rationale:**
- Native compilation to native binaries provides superior timing accuracy for critical metronome feature
- Built-in security and HIPAA compliance features reduce regulatory overhead
- Desktop support (Windows/macOS) enables clinical workstation use
- Strong integration with .NET ecosystem (developer has C# experience)

**Supporting Stack:**
- SQLite (encrypted local database for offline-first)
- Plugin.Maui.Audio (cross-platform audio abstraction)
- CommunityToolkit.MaUI (60fps animations, performance optimization)
- Syncfusion UI (professional components, reduced development time)
- iTextSharp/CsvHelper (PDF + CSV export)

### Critical Features

**Table Stakes (Must-Have for v1):**
1. Audio metronome (100-120 BPM, precise timing)
2. Visual metronome (animated, synchronized with audio)
3. 2-minute CPR cycle timer (AHA protocol requirement)
4. Medication timers (epinephrine, amiodarone protocols)
5. Event logging (automatic timestamp capture)
6. Rhythm selection (VF/VT, AEA, Asystole, Bradycardia, Tachycardia)
7. H's and T's checklist (systematic reversible cause tracking)
8. PDF + CSV export (clinical documentation)

**Competitive Differentiators:**
- Context-aware audio reminders ("Time for epinephrine?") vs. passive timers
- Offline-first architecture (many competitors assume connectivity)
- Multi-role architecture preparation (future v2+ expansion)
- Adaptive metronome visualization (high-contrast, urgency cues)

### Architecture Approach

**Offline-First Pattern:**
- Local SQLite database (encrypted) as primary storage
- All core features work without internet
- Optional cloud sync when connectivity available
- Background sync, non-blocking to UI

**Architectural Patterns:**
- MVVM (Model-View-ViewModel) for testability
- Repository pattern for data access abstraction
- Dependency injection for service management
- Async/await for all I/O operations (prevent UI freeze)

**Component Layering:**
- Presentation Layer: XAML Views + ViewModels
- Business Logic Layer: Domain services (Metronome Engine, Timer Coordinator, Medication Scheduler, Reminder Engine)
- Data Access Layer: Repositories with SQLite
- Storage Layer: Encrypted SQLite database

### High-Risk Pitfalls

**Timing-Critical Issues:**
- Metronome drift (audio stack latency, low-precision timers) → Use Stopwatch, test on physical devices
- Audio interruption handling (phone calls, other apps) → Implement audio focus handling

**HIPAA Compliance:**
- Unencrypted data storage → Use AES-256 encryption, platform secure storage for keys
- PHI in logs → Never log patient data, sanitize all debug output
- No database migrations → Data loss on updates → Implement migration system by v1.x

**Emergency UX Challenges:**
- Over-complex UI → Large buttons, high contrast, minimal screens
- Offline failures → Design offline-first, clear connectivity status
- Small touch targets → Minimum 44x44 points, test with gloves

**Clinical Accuracy:**
- Incorrect protocol timing → Source from AHA ACLS 2020 official guidelines only
- Missing timestamp precision → Store milliseconds, ensure clinical review capability
- No rhythm-specific protocols → VF/VT vs AEA vs Asystole have different timing

## Implementation Guidance

### Phase Priorities

**Phase 1 (Core Functionality):**
1. Implement .NET MAUI project structure with MVVM
2. Build encrypted SQLite database schema (Sessions, Events, Config)
3. Create Metronome Engine with Stopwatch precision
4. Implement Timer Coordinator (2-minute cycles, medication timers)
5. Add audio metronome using Plugin.Maui.Audio
6. Create visual metronome with 60fps animations
7. Build event logging system with millisecond timestamps
8. Implement rhythm selection (VF/VT, AEA, etc.)
9. Add medication scheduler based on ACLS 2020 protocols
10. Create basic UI (minimal screens, large buttons)

**Phase 2 (Validation & Polish):**
1. Test metronome timing accuracy on physical iOS/Android devices
2. Verify HIPAA encryption (database file security test)
3. Add H's and T's checklist with auto-prompting
4. Implement PDF export using iTextSharp
5. Implement CSV export using CsvHelper
6. Add offline mode indicators
7. Optimize UI for emergency conditions (gloves, low-light)
8. Implement audio interruption handling
9. Add session history with lazy loading
10. Healthcare provider beta testing

**Phase 3 (Differentiators):**
1. Build context-aware reminder engine (proactive prompts)
2. Add adaptive visualizer (color-coded urgency)
3. Implement optional cloud sync architecture
4. Prepare multi-role data structure (future expansion)

### Quality Gates

**Before Phase 1 Complete:**
- [ ] Metronome BPM verified within ±1 BPM on physical devices
- [ ] SQLite database encrypted, cannot be opened outside app
- [ ] All core features work in airplane mode (offline)
- [ ] Timestamp precision tested (milliseconds captured)

**Before Phase 2 Complete:**
- [ ] Export tested on actual iOS/Android devices (file accessible)
- [ ] Audio interruption handling verified (phone call mid-session)
- [ ] Healthcare provider feedback collected from beta test
- [ ] Cross-reference with AHA ACLS 2020 guidelines (no discrepancies)

## Risk Mitigation

### High-Risk Items

1. **Metronome Timing Accuracy**
   - Risk: Clinical failure if timing drifts
   - Mitigation: Unit tests + physical device testing, calibration in production

2. **HIPAA Compliance**
   - Risk: Fines, loss of trust
   - Mitigation: Security audit, encrypted storage, no PHI in logs

3. **Offline Reliability**
   - Risk: App unusable during emergencies
   - Mitigation: Offline-first design, airplane mode testing

### Contingency Plans

- If metronome timing issues detected → Release calibration update immediately
- If HIPAA violation discovered → Notify users, encryption patch, credit monitoring
- If offline failure → Emergency mode toggle, disable sync, prioritize local storage

## Competitive Analysis

**Existing Apps:**
- CARMA, AHA ACLS App, Code Blue, ACLS Helper

**Our Advantages:**
- Context-aware reminders (proactive vs. passive)
- Offline-first with smart sync (many assume connectivity)
- Architecture prepared for multi-role expansion
- .NET MAUI native performance (over React Native's interpreted approach)

**Gaps to Address:**
- Ensure metronome matches competitor quality (CARMA is industry standard)
- Match feature set (all competitors have basic timers + logging)
- Exceed in differentiators (context reminders are unique value prop)

## Success Metrics

**Technical:**
- Metronome accuracy: ±1 BPM target
- Audio reliability: 100% uptime during session (no glitches)
- Offline functionality: 100% of features work without network

**Clinical:**
- Protocol accuracy: 100% alignment with AHA ACLS 2020
- Timestamp precision: Millisecond capture in 100% of events
- HIPAA compliance: Zero PHI exposure, encrypted storage

**User Experience:**
- Time to first compression: <5 seconds from app launch
- Time to log medication: <3 seconds from administration
- Export generation: <10 seconds for 30-minute session

## Next Steps

1. Confirm technology stack selection (.NET MAUI vs React Native) with team expertise
2. Set up .NET MAUI development environment
3. Implement Phase 1 features following priority order
4. Conduct Phase 1 testing on physical devices (iOS + Android)
5. Execute Phase 2 validation with healthcare providers
6. Plan Phase 3 differentiator implementation

---

*Research Summary completed: 24/03/2026*
*Confidence: HIGH — Stack, features, architecture, and pitfalls research complete*