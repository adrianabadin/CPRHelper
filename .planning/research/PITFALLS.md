# Pitfalls Research

**Domain:** Mobile Medical ACLS Application
**Researched:** 24/03/2026
**Confidence:** HIGH

## Critical Pitfalls

### Pitfall 1: Timing Inaccuracy in Metronome

**What goes wrong:**
Audio metronome drifts from set BPM (e.g., set to 110 but actually plays at 105). This leads to suboptimal CPR compression rate, directly impacting patient outcomes.

**Why it happens:**
Developers use low-precision timers (e.g., Thread.Sleep) or don't account for audio stack latency. JavaScript-based solutions (React Native) may have event loop overhead. Audio plugin implementation varies between iOS/Android.

**How to avoid:**
- Use Stopwatch class for high-precision timing
- Test timing accuracy across platforms (iOS vs Android audio stacks)
- Calibrate audio playback with actual BPM measurements
- Use native audio plugins (not web-based solutions)

**Warning signs:**
Manual BPM verification shows consistent deviation (>1 BPM). User reports "metronome feels slow/fast."

**Phase to address:**
Phase 1 — Metronome is core value. Must be verified in development.

---

### Pitfall 2: HIPAA Violation in Data Storage

**What goes wrong:**
Patient data (session logs) stored unencrypted on device. If device is lost/stolen, PHI is exposed. Violation results in fines and loss of trust.

**Why it happens:**
Developers use plain SQLite or JSON files. Assume "device is secure enough." Don't implement encryption at rest.

**How to avoid:**
- Encrypt SQLite database at rest (AES-256 standard)
- Use platform-provided secure storage (iOS Keychain, Android EncryptedSharedPreferences) for encryption keys
- Never log PHI in plain text files or debug output
- Audit data flow for PHI exposure

**Warning signs:**
Database files are readable with any SQLite browser. Session export contains patient names or identifiers.

**Phase to address:**
Phase 1 — Data storage is foundational. HIPAA compliance from day one.

---

### Pitfall 3: Failure During Offline Operation

**What goes wrong:**
App assumes connectivity. User opens app during emergency, no WiFi, app crashes or shows "syncing" spinner. No access to metronome or timers. Critical failure.

**Why it happens:**
Developers test only on development machines with reliable internet. Don't design for offline-first architecture. Cloud APIs called without fallback to local storage.

**How to avoid:**
- Design offline-first: all core features work without network
- Use local SQLite as primary storage, cloud as optional backup
- Show clear "offline" status indicator in UI
- Sync in background when connectivity restores (non-blocking)

**Warning signs:**
App hangs on startup waiting for server. Features unavailable without WiFi. User reports "crashes during emergencies."

**Phase to address:**
Phase 1 — Offline-first is requirement. Test with airplane mode enabled.

---

### Pitfall 4: Over-Complex UI in High-Stress Environments

**What goes wrong:**
Screen cluttered with too many controls, small fonts, low contrast. Healthcare provider cannot quickly find "start metronome" or "administer medication." Delays treatment.

**Why it happens:**
Developers design for calm office environment. Don't consider: low light, high stress, gloves on fingers, time pressure, noise.

**How to avoid:**
- Large touch targets (min 44x44 points)
- High contrast colors (especially in dark mode)
- Minimal screens: one primary action per screen
- Test with gloves on physical devices
- Use haptic feedback for button presses
- Audio prompts as backup to visual cues

**Warning signs:**
User testing shows confusion about which button to tap. Healthcare providers report "can't see screen in low light."

**Phase to address:**
Phase 2 — UI/UX refinement. After metronome works, optimize for emergency use.

---

### Pitfall 5: Incorrect ACLS 2020 Protocol Implementation

**What goes wrong:**
App suggests "epinephrine every 5 minutes" when ACLS 2020 specifies "epinephrine every 3-5 minutes" or incorrect medication dosages. Misguides healthcare provider.

**Why it happens:**
Developers rely on outdated internet sources or guess at protocols. Don't reference official AHA guidelines. Implement generic CPR timing instead of rhythm-specific protocols.

**How to avoid:**
- Source all medication timing from AHA ACLS 2020 official guidelines
- Implement rhythm-specific protocols (VF/VT vs AEA vs Asystole are different)
- Version-control protocol logic to track guideline changes
- Allow user to override or disable automated reminders (professional discretion)

**Warning signs:**
Healthcare provider questions "why is it saying X?" about timing. Discrepancy with printed AHA algorithms.

**Phase to address:**
Phase 1 — Medication scheduler is core feature. Must be protocol-accurate.

---

### Pitfall 6: Data Loss on App Update or Device Change

**What goes wrong:**
User updates app version → all session history disappears. User gets new phone → cannot transfer previous logs. Loss of clinical records.

**Why it happens:**
No database migration strategy. SQLite file location changes between app versions. No export/import for device transfer.

**How to avoid:**
- Implement SQLite migration system (versioned schema changes)
- Provide explicit export/import functions (PDF/CSV) before updates
- Use device backup APIs (iOS iCloud, Android backup) where possible
- Test upgrade path: v1 → v2 data must migrate

**Warning signs:**
App reinstall or update shows empty history list. User reports "lost all my sessions."

**Phase to address:**
Phase 2 — Data persistence enhancement. MVP can skip, but v1.x must include migrations.

---

### Pitfall 7: Inadequate Testing of Audio Playback

**What goes wrong:**
Metronome audio glitches, stops, or plays at wrong rate on production devices. Works fine in development simulator.

**Why it happens:**
Developers test only in emulators, not on physical devices with actual speaker hardware. Don't handle audio interruption (phone call, other app audio). Don't test with OS audio mixing.

**How to avoid:**
- Test on physical iOS and Android devices
- Test with phone calls in progress (audio interruption handling)
- Test with other apps playing audio (audio focus)
- Test speaker volume levels (must be audible in noisy environment)
- Test with headphones vs device speaker

**Warning signs:**
User reports "metronome stopped working mid-session." Audio quality varies across devices.

**Phase to address:**
Phase 1 — Audio reliability is essential. Test on multiple physical devices.

---

### Pitfall 8: Missing Timestamp Precision in Event Logging

**What goes wrong:**
Session logs show events with minute-level timestamps (e.g., "Medication given at 14:35" instead of "14:35:42"). Inadequate for clinical review and quality improvement.

**Why it happens:**
Developers use DateTime.Now without milliseconds. Don't capture precise timing needed for medication administration intervals.

**How to avoid:**
- Store all events with DateTime.UtcNow (include milliseconds)
- Display timestamps appropriately in UI (user may want seconds or milliseconds)
- Export with millisecond precision in CSV (PDF can format for readability)
- Use high-precision timing for metronome beats

**Warning signs:**
Session export shows same timestamp for multiple events. Reviewers cannot determine actual sequence.

**Phase to address:**
Phase 1 — Event logging is core. Millisecond precision required.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Hardcoded medication timing strings | Faster to implement v1 | Cannot update protocols without code change | Never - protocols change, app becomes outdated |
| No database migrations | Simpler initial schema | Data loss on updates | MVP only, must add by v1.x |
| Skip audio testing on physical devices | Faster development, just use emulator | Production failures, user complaints | Never - critical feature requires real device testing |
| Inline string literals for UI text | No need for localization | Cannot translate to other languages | MVP only, must add by v1.x |
| Manual error code instead of exception handling | Fewer try/catch blocks | Crashes in production, poor debugging | Never - exceptions must be logged and handled |

## Integration Gotchas

Common mistakes when connecting to external services.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| (Future) Cloud Sync API | Sync immediately on every action (blocking UI) | Queue changes in background, sync when idle and connected |
| (Future) ECG Bluetooth | Assume device always connected, no error handling | Handle disconnection gracefully, manual rhythm fallback |
| Audio System | Assume audio always plays, no interruption handling | Handle audio focus loss, resume playback after interruption |
| File Export | Write to app bundle (read-only on some devices) | Write to user-accessible directory (Documents/Downloads) |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|-----------------|
| No lazy loading for session history | App startup slow, scrolling lags with 100+ sessions | Implement pagination/virtualization | 50+ sessions in database |
| Synchronous PDF export | UI freeze for 30+ seconds | Async/await with progress indicator | Session > 10 minutes or 100+ events |
| No database indexing | Session history query slow (seconds) | Add indexes on Timestamp, SessionId fields | 100+ sessions or 10,000+ events |
| Animations on main thread | Choppy metronome visualizer | Use CommunityToolkit.Maui optimizations, 60fps targeting | Any animation |

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| Logging PHI to console/debug output | Accidental exposure in crash reports | Never log patient data, sanitize all logs before output |
| Storing encryption keys in plain config | Key extraction if device is rooted/jailbroken | Use platform secure storage (Keychain/EncryptedSharedPreferences) |
| Sending unencrypted data over network | Man-in-the-middle attacks | Always use TLS 1.3+ for any API communication |
| Not verifying SSL certificates | MITM vulnerability | Use certificate pinning for critical APIs |
| Allowing screenshots of session data | PHI leaks via sharing | Disable screenshot capability for sensitive screens (if platform allows) |

## UX Pitfalls

Common user experience mistakes in this domain.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Small buttons (44x44 points minimum) | Missed taps with gloves, frustration | Large touch targets, spacing between buttons |
| Low contrast in dark mode | Cannot see screen in low-light emergency room | WCAG AA contrast ratios, test in dark conditions |
| Too many actions on one screen | Confusion about which button to press | One primary action per screen, secondary in submenu |
| No haptic feedback | Uncertainty if button registered | Add vibration on button press |
| No offline indicator | User doesn't know why sync isn't working | Always show connectivity status, clear offline message |
| Complex multi-step navigation | Wastes time finding features | Flat navigation structure, max 3 taps to any feature |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Metronome Audio**: Often missing audio interruption handling → verify phone call mid-session, app must resume/notify
- [ ] **Timer Accuracy**: Often missing drift testing over 10+ minutes → verify BPM stays within ±1 BPM for extended sessions
- [ ] **HIPAA Encryption**: Often missing key management → verify database file cannot be opened without app
- [ ] **Export Functionality**: Often missing export to accessible location → verify PDF/CSV can be opened/shared from device
- [ ] **Offline Operation**: Often missing sync conflict handling → verify app works 100% in airplane mode
- [ ] **Protocol Accuracy**: Often missing rhythm-specific timing → verify VF/VT vs AEA vs Asystole have different reminder schedules
- [ ] **Error Handling**: Often missing logging → verify app catches and logs all exceptions, never crashes silently

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Metronome timing drift | LOW | Calibration update: measure actual BPM, adjust audio delay, prompt user to update app |
| HIPAA violation (data exposed) | HIGH | Notify affected users, offer credit monitoring, implement encryption patch immediately, re-encrypt data |
| Offline failure | MEDIUM | Add offline mode toggle, prioritize local storage, patch sync logic for background operation |
| Protocol errors | HIGH | Hotfix release with corrected timing, emergency communication to users, revert to manual reminders if app cannot be updated immediately |
| Data loss | HIGH | Offer session reconstruction from any backups, apology and process review, implement export/import immediately |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Metronome timing inaccuracy | Phase 1 | Unit tests for timing drift, manual verification on physical devices, user beta testing |
| HIPAA encryption failure | Phase 1 | Security audit, attempt to open database file outside app, verify encryption keys stored securely |
| Offline operation failure | Phase 1 | Test with airplane mode, verify all features work without network, no blocking sync calls |
| Over-complex UI | Phase 2 | Healthcare provider testing with gloves, in low-light conditions, user feedback rounds |
| Protocol implementation errors | Phase 1 | Cross-reference with AHA ACLS 2020 official guidelines, clinical validation by medical professionals |
| Data loss on updates | Phase 2 | Migration tests (v1 → v2), upgrade path verification, export before update prompt |
| Audio playback issues | Phase 1 | Physical device testing (iOS + Android), audio interruption handling tests, speaker volume verification |
| Timestamp precision | Phase 1 | Export CSV and verify milliseconds present, unit test DateTime.UtcNow capture |
| Performance bottlenecks | Phase 2 | Load test with 100+ sessions, profile PDF export with large sessions |
| Security mistakes | Phase 1 | Security audit, penetration testing, code review for PHI exposure |

## Sources

- Emergency medical app development challenges research
- Common mistakes in mobile healthcare apps
- AHA ACLS 2020 Guidelines accuracy requirements
- HIPAA compliance best practices for mobile apps
- Research on real-world emergency app failures

---
*Pitfalls research for: Mobile Medical ACLS Application*
*Researched: 24/03/2026*