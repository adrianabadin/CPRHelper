# Feature Research

**Domain:** Mobile ACLS Cardiac Arrest Application
**Researched:** 24/03/2026
**Confidence:** HIGH

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| **Audible Metronome** | CPR requires consistent 100-120 compressions/minute. Standard in all ACLS apps. | LOW | Audio beep at configurable rate (default 110 BPM). Must be precise timing. |
| **Visual Metronome** | Visual confirmation of rhythm, especially useful when audio is muted. | LOW | Animated element synchronized with audio beats. Can be circle pulse, bar, or counter. |
| **2-minute Cycle Timer** | AHA ACLS 2020 specifies 2-minute CPR cycles before rhythm check. | MEDIUM | Automatic countdown with alert at end. Must track elapsed total time. |
| **Medication Timers** | Epinephrine every 3-5 minutes, Amiodarone protocols. | MEDIUM | Independent timers for each medication. User confirms administration. |
| **Event Logging** | Clinical documentation requirement. All events must be timestamped. | MEDIUM | Automatic timestamp capture for medications, rhythm changes, CPR cycles, defibrillations. |
| **Rhythm Selection** | ACLS algorithms depend on rhythm (VF/VT, AEA, Asystole, Bradycardia, Tachycardia). | MEDIUM | Dropdown/selection interface. Changes available reminders and protocols. |
| **H's and T's Checklist** | Systematic approach to identifying reversible causes. | LOW | Toggle items as "discussed" or "ruled out". Auto-prompt at appropriate times. |
| **Session Export** | Clinical workflows require archiving and review. | MEDIUM | PDF for human-readable reports, CSV for data analysis. |

### Differentiators (Competitive Advantage)

Features that set product apart. Not required, but valuable.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| **Context-Aware Audio Reminders** | Proactive guidance vs. passive timing. Competitors mostly do static timers. | HIGH | "It's been 2 minutes. Time for epinephrine?" "Have you ruled out H and T causes?" Intelligent timing based on rhythm and last actions. |
| **Offline-First with Smart Sync** | Critical for emergencies where WiFi is unreliable. Many competitors assume connectivity. | HIGH | Full functionality offline. Sync when connectivity restored. Conflict resolution for overlapping edits. |
| **Multi-Role Architecture Prepared** | Future expansion to support full team (compressor, airway, recorder, etc.). | HIGH | Current v1 is leader-only, but architecture supports future role-specific screens. Competitors are leader-only. |
| **Adaptive Metronome Visualization** | Large, high-contrast visual cues for stressful environments. | MEDIUM | Color-coded urgency (green when on-track, red when behind). Supports low-light/low-visibility scenarios. |
| **Automatic ECG Integration Hooks** | Future potential for direct device integration vs. manual rhythm entry. | HIGH | Bluetooth LE API integration points. Bluetooth ECG devices could auto-populate rhythm. |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| **Real-time ECG Waveform Display** | Looks impressive, shows "medical" sophistication | Requires FDA medical device classification. Massive regulatory overhead. Audio/metronome is sufficient for guidance. | Manual rhythm selection (VF/VT, AEA, etc.) - avoids device classification. |
| **Voice Commands** | Hands-free operation during CPR | Unreliable in noisy emergency environments. Recognition errors could cause medication errors. | Large touch buttons with haptic feedback. |
| **Cloud-Only Storage** | Easy to access from anywhere | Fails during emergencies with no connectivity. Critical data loss. | Offline-first with local SQLite. Optional cloud sync for backup. |
| **Social Sharing of Session Data** | "Share to Facebook" type features | HIPAA violation risk. Patient data cannot be shared casually. | Secure email export to designated clinical contacts. HIPAA-compliant sharing. |

## Feature Dependencies

```
[Audio Metronome System]
    ├──requires──> [Local Storage (SQLite)]
    ├──requires──> [Timer Engine]
    └──enhanced-by──> [Visual Metronome]

[Event Logging System]
    ├──requires──> [Local Storage (SQLite)]
    ├──enhanced-by──> [H's and T's Tracker]
    └──outputs──> [PDF/CSV Export]

[Medication Timer System]
    ├──requires──> [Timer Engine]
    ├──enhanced-by──> [Context-Aware Reminders]
    └──logs-to──> [Event Logging System]

[Rhythm Selection]
    ├──triggers──> [Context-Aware Reminders]
    └──affects──> [Medication Timer System]

[Multi-Role Architecture] (future)
    ├──requires──> [Session Sharing]
    └──enhanced-by──> [Device-to-Device Communication]
```

### Dependency Notes

- **[Audio Metronome System] requires [Timer Engine]:** Precise timing is foundation. Timer engine must handle sub-millisecond accuracy for 100-120 BPM.
- **[Event Logging System] outputs to [PDF/CSV Export]:** Export formats depend on structured log data. SQLite must store all events with timestamps.
- **[Rhythm Selection] triggers [Context-Aware Reminders]:** Different rhythms have different medication protocols. VF/VT needs defibrillation reminders, AEA needs immediate CPR focus.

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate concept.

- [ ] **Audio + Visual Metronome** — Core timing guidance for CPR compressions. Essential for metronome value proposition.
- [ ] **2-minute Cycle Timer** — Enforces AHA protocol timing structure. Without this, just a metronome app.
- [ ] **Medication Timers** — Tracks epinephrine, amiodarone administration schedules. Standard in all ACLS apps.
- [ ] **Event Logging** — Records all actions with timestamps. Clinical documentation requirement.
- [ ] **Rhythm Selection** — Enables context-aware reminders. VF/VT vs AEA protocols differ.
- [ ] **H's and T's Checklist** — Systematic reversible cause tracking. Differentiating feature vs basic timers.
- [ ] **PDF + CSV Export** — Clinical workflow integration. Allows archiving and review.
- [ ] **Offline-First** — Emergency environment requirement. All features must work without connectivity.

### Add After Validation (v1.x)

Features to add once core is working.

- [ ] **Context-Aware Audio Reminders** — Proactive guidance ("Time for epinephrine?"). Validates that leaders find passive timers insufficient.
- [ ] **Session History** — List past resuscitation events. Review and learning tool.
- [ ] **Smart Sync** — Optional cloud sync when connectivity available. Backup and multi-device access.

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] **Multi-Role Screens** — Compressor, airway, recorder views. Requires device-to-device communication.
- [ ] **Bluetooth ECG Integration** — Automatic rhythm detection. Hardware integration and FDA classification.
- [ ] **Clinical Analytics** — Performance metrics, compliance dashboards. Administrative features.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Audio + Visual Metronome | HIGH | LOW | P1 |
| 2-minute Cycle Timer | HIGH | MEDIUM | P1 |
| Medication Timers | HIGH | MEDIUM | P1 |
| Event Logging | HIGH | MEDIUM | P1 |
| Rhythm Selection | HIGH | LOW | P1 |
| H's and T's Checklist | MEDIUM | LOW | P1 |
| PDF + CSV Export | MEDIUM | MEDIUM | P1 |
| Offline-First | HIGH | HIGH | P1 |
| Context-Aware Audio Reminders | MEDIUM | HIGH | P2 |
| Session History | MEDIUM | MEDIUM | P2 |
| Smart Sync | LOW | HIGH | P2 |
| Multi-Role Screens | MEDIUM | HIGH | P3 |
| Bluetooth ECG Integration | HIGH | VERY HIGH | P3 |
| Clinical Analytics | LOW | HIGH | P3 |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have, future consideration

## Competitor Feature Analysis

| Feature | CARMA | AHA ACLS App | Code Blue | ACLS Helper | Our Approach |
|---------|---------|----------------|-----------|--------------|--------------|
| Metronome | ✓ | ✓ | ✓ | ✓ | ✓ with adaptive visualization (differentiator) |
| 2-min Timer | ✓ | ✓ | ✓ | ✓ | ✓ with context-aware reminders |
| Med Timers | ✓ | ✓ | ✓ | ✓ | ✓ + context-aware |
| H's and T's | ✓ | ✓ | ✗ | ✓ | ✓ + auto-prompting |
| PDF Export | ✓ | ✓ | ✓ | ✓ | ✓ + CSV dual format |
| Offline-First | ✓ | Unclear | ✓ | Unclear | ✓ with smart sync (differentiator) |
| Context Reminders | ✗ | ✗ | ✗ | ✗ | ✓ (major differentiator) |
| Multi-Role | ✗ | ✗ | ✗ | ✗ | Architecture prepared, v2+ |

## Sources

- CARMA (Cardiac Arrest Resuscitation Mobile Application) website and features
- AHA ACLS App official description and features
- Code Blue app store listing and feature breakdown
- ACLS Helper app documentation
- Competitor comparison research
- AHA ACLS 2020 Guidelines requirements

---
*Feature research for: Mobile ACLS Cardiac Arrest Application*
*Researched: 24/03/2026*