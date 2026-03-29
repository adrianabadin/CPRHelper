# Phase 03: Protocol Guidance - Research

**Researched:** 29/03/2026
**Domain:** AHA ACLS 2020 protocol reminder integration into existing MVVM event flow
**Confidence:** HIGH

## Summary

This phase adds context-aware visual reminders based on AHA ACLS 2020 protocol during active resuscitation. The implementation integrates entirely into the existing `MainViewModel.cs` event flow — augmenting the pulse-check popup with additional protocol suggestion lines and replacing/extending the rhythm-change handler with ACEPTAR/RECHAZAR popups for all rhythm types.

The codebase already has all the infrastructure needed: DisplayAlert two-button pattern (line 156), dynamic suggestion list builder in `OnPulseCheckDue` (line 124), PropertyChanged handler for rhythm changes (line 33), and `LogCustomEventCommand` for event logging. The work is additive — no new services, no new views, no new models required. The primary challenge is handling async DisplayAlert calls from the synchronous PropertyChanged handler context, and ensuring popups don't queue/block each other during rapid rhythm transitions.

**Primary recommendation:** Keep all logic in `MainViewModel.cs` using existing patterns. Add a cycle counter field, extend the suggestion builder in `OnPulseCheckDue`, extract rhythm popup logic into an `async Task` method called via fire-and-forget from the PropertyChanged handler. No new classes needed.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **Sin audio** — NO se agregan recordatorios de audio en esta fase. Solo popups visuales (DisplayAlert). Audio quedó explícitamente diferido a v2 (AUDI-04, TIME-04).
- **Recordatorios en popup de check de pulso (cada 2 min)** — Se agregan al popup de check de pulso existente (OnPulseCheckDue). Los nuevos recordatorios se agregan como líneas adicionales en el mensaje ANTES de las sugerencias de medicación.
- **Recordatorios al cambiar de ritmo** — Se muestran como popups separados (DisplayAlert) con botones ACEPTAR/RECHAZAR al momento del cambio de ritmo.
- **Interacción ACEPTAR/RECHAZAR** — Todos los recordatorios son popups DisplayAlert con dos botones: "ACEPTAR" y "RECHAZAR". ACEPTAR → registra en event log. RECHAZAR → registra en event log. Ambos cierran el popup y continúan el flujo. No hay consecuencia funcional.
- **Tracking de ciclo para "primer ciclo"** — Se necesita un contador de ciclos. Se resetea en StartCode() y se incrementa en NewCycle(). El recordatorio de IV/IO se muestra solo en el primer check de pulso.
- **Defibrilación en event log** — Ya funciona correctamente. No requiere cambios.

### Specific Reminder Rules (Locked)

#### Pulse Check Reminders (added to existing popup):
| Condition | Text | Frequency |
|-----------|------|-----------|
| Primer ciclo (ciclo 0→1) | "¿Colocó acceso IV/IO?" | Solo primera vez |
| Siempre | "¿Rotar compresor?" | Cada check de pulso |
| H's y T's sin revisar | "Revisar H's y T's pendientes: {lista de nombres}" | Cada check si quedan pendientes |
| Timer Adrenalina >4min | "¿Hora de Adrenalina?" | Ya existe (02.1.1) |
| Timer Amiodarona >4min + TV/FV + dosis<2 | "¿Hora de Amiodarona? (Xra dosis: Xmg)" | Ya existe (02.1.1) |

#### Rhythm Change Reminders (separate popups):
| New Rhythm | Popup Text |
|------------|-----------|
| **AESP** | "Buscar causas reversibles\nConsidere revisar H's y T's" |
| **Asistolia** | "Buscar causas reversibles\nConsidere revisar H's y T's" |
| **TV** | "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)" |
| **FV** | "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)" |
| **RCE** | "RCE alcanzado\n• Mantener vía aérea y ventilación\n• Monitorear ritmo y presión arterial\n• Obtener ECG 12 derivaciones\n• Considerar objetivo temp 32-36°C\n• Considerar causas reversibles" |

Note: TV/FV popups REPLACE the existing "Ritmo Desfibrilable" popup (lines 39-44 of MainViewModel.cs).

### Deferred Ideas (OUT OF SCOPE)
- **Audio de recordatorios** — v2 requirements AUDI-04, TIME-04
- **Grabar sesión en base de datos local** — nueva fase propuesta
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| REGI-04 | El sistema genera recordatorios contextuales basados en el protocolo ACLS 2020 | All integration points identified in existing codebase. DisplayAlert two-button pattern verified. Rhythm popup rules mapped to CardiacRhythm enum. Pulse check suggestions mapped to existing suggestion builder. Event logging via LogCustomEventCommand confirmed. |
</phase_requirements>

## Standard Stack

### Core (All Already in Project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CommunityToolkit.Mvvm | 8.2.2 | MVVM source generators ([ObservableProperty], [RelayCommand]) | Already used throughout codebase |
| CommunityToolkit.Maui | 9.0.0 | Toast alerts (used for charging warning) | Already in project |
| Microsoft.Maui.Controls | 9.0.120 | DisplayAlert API for popups | Core MAUI framework — no install needed |

### No New Dependencies Required
This phase uses exclusively existing infrastructure. No new NuGet packages needed.

## Architecture Patterns

### Recommended Code Changes (Single File: MainViewModel.cs)

```
AclsTracker/
├── ViewModels/
│   └── MainViewModel.cs    ← ALL changes here
├── Models/
│   ├── CardiacRhythm.cs    ← No changes (enum already has all rhythms)
│   ├── HsAndTsItem.cs      ← No changes (IsChecked, IsDismissed, Name exist)
│   └── TimerModel.cs       ← No changes (IsOverThreshold already works)
└── ViewModels/
    └── EventRecordingViewModel.cs  ← No changes (HsAndTsItems, CurrentRhythm, LogCustomEventCommand exist)
```

### Pattern 1: Fire-and-Forget Async from PropertyChanged Handler
**What:** The PropertyChanged handler cannot be async. Extract async popup logic into a separate async method and call with fire-and-forget.
**When to use:** Rhythm change popups that need ACEPTAR/RECHAZAR response.
**Verified in codebase:** Existing pattern at line 41 uses `_ = DisplayAlert(...)` for fire-and-forget.

```csharp
// Existing pattern (lines 33-46) — synchronous handler
EventRecording.PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(EventRecordingViewModel.CurrentRhythm))
    {
        IsAmiodaronaEnabled = EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV;
        _ = HandleRhythmChangeAsync(EventRecording.CurrentRhythm); // fire-and-forget
    }
};

// New extracted async method
private async Task HandleRhythmChangeAsync(CardiacRhythm newRhythm)
{
    (string title, string message)? popup = newRhythm switch
    {
        CardiacRhythm.AESP      => ("Protocolo ACLS", "Buscar causas reversibles\nConsidere revisar H's y T's"),
        CardiacRhythm.Asistolia => ("Protocolo ACLS", "Buscar causas reversibles\nConsidere revisar H's y T's"),
        CardiacRhythm.TV        => ("Ritmo Desfibrilable", "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)"),
        CardiacRhythm.FV        => ("Ritmo Desfibrilable", "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)"),
        CardiacRhythm.RCE       => ("RCE Alcanzado", "RCE alcanzado\n• Mantener vía aérea y ventilación\n• Monitorear ritmo y presión arterial\n• Obtener ECG 12 derivaciones\n• Considerar objetivo temp 32-36°C\n• Considerar causas reversibles"),
        _ => null
    };

    if (popup is not null)
    {
        bool accepted = await Application.Current!.MainPage!
            .DisplayAlert(popup.Value.title, popup.Value.message, "ACEPTAR", "RECHAZAR");

        string decision = accepted ? "aceptada" : "rechazada";
        // Use first 50 chars of message as summary
        string summary = popup.Value.message.Split('\n')[0];
        EventRecording.LogCustomEventCommand.Execute($"Recomendación {decision}: {summary}");
    }
}
```

**Confidence:** HIGH — pattern already exists in codebase (line 41), official MAUI docs confirm DisplayAlert returns `bool`.

### Pattern 2: Extend Existing Suggestion Builder in OnPulseCheckDue
**What:** Add new suggestion lines to the existing `List<string> suggestions` before medication lines.
**When to use:** Pulse check reminders (IV/IO, rotar compresor, H's y T's).
**Verified in codebase:** Lines 124-146 already build dynamic suggestion list.

```csharp
// In OnPulseCheckDue, BEFORE the existing medication suggestions:
var suggestions = new List<string>();

// NEW: Cycle-specific reminders (Protocol Guidance)
if (_cycleCount == 1) // First cycle only (after first NewCycle or first pulse check)
{
    suggestions.Add("¿Colocó acceso IV/IO?");
}

// NEW: Always show compressor rotation reminder
suggestions.Add("¿Rotar compresor?");

// NEW: H's and T's pending review
var pendingHsTs = EventRecording.HsAndTsItems
    .Where(i => !i.IsChecked && !i.IsDismissed)
    .Select(i => i.Name)
    .ToList();
if (pendingHsTs.Count > 0)
{
    suggestions.Add($"Revisar H's y T's pendientes: {string.Join(", ", pendingHsTs)}");
}

// EXISTING: Medication suggestions (Adrenalina, Amiodarona) — no changes
if (Timer.Timers[3].IsOverThreshold) { ... }
if (Timer.Timers[4].IsOverThreshold && ...) { ... }
```

**Confidence:** HIGH — direct extension of existing pattern at lines 124-146.

### Pattern 3: Cycle Counter Field
**What:** Simple `int` field tracking CPR cycles, reset in StartCode, incremented in NewCycle.
**When to use:** IV/IO reminder only shows on first cycle.

```csharp
// Add field alongside existing _amiodaronaDoseCount
private int _amiodaronaDoseCount;
private int _cycleCount;  // NEW

// In StartCode():
_cycleCount = 0;

// In NewCycle():
_cycleCount++;
```

**Note:** The IV/IO reminder should show when `_cycleCount == 0` at the time of OnPulseCheckDue (before NewCycle is called). After NewCycle is called, _cycleCount becomes 1. So the check should be `_cycleCount == 0` for "first pulse check ever".

**Confidence:** HIGH — trivial field addition matching existing `_amiodaronaDoseCount` pattern.

### Anti-Patterns to Avoid
- **Don't create a ProtocolGuidanceService for this:** All logic is UI-popup-driven and tightly coupled to MainViewModel's timer/rhythm state. A separate service adds DI complexity for zero benefit in this phase.
- **Don't use Toast for protocol reminders:** Toast is non-blocking and auto-dismisses. Protocol reminders need explicit ACEPTAR/RECHAZAR — must use DisplayAlert.
- **Don't block the UI thread waiting for popup response:** The pulse check popup already blocks (await). Rhythm change popups must use fire-and-forget to avoid blocking the PropertyChanged handler.
- **Don't show rhythm popups for `Ninguno`:** The initial/empty rhythm state should not trigger any popup. The `switch` expression with `_ => null` handles this.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Popup with two buttons | Custom modal page | `DisplayAlert(title, message, "ACEPTAR", "RECHAZAR")` | Native platform rendering, accessible, already used in codebase |
| Event logging | New logging mechanism | `EventRecording.LogCustomEventCommand.Execute(string)` | Existing service with timestamps, event type, and Observable collection |
| Timer threshold checking | Custom timer logic | `Timer.Timers[N].IsOverThreshold` | Already handles all timer types with correct thresholds |
| H's and T's filtering | New query logic | `HsAndTsItems.Where(!IsChecked && !IsDismissed)` | LINQ on existing ObservableCollection |

## Common Pitfalls

### Pitfall 1: Android Dismiss-by-Tap-Outside Returns False
**What goes wrong:** On Android, tapping outside the alert dialog dismisses it. This returns `false` (same as pressing the second button "RECHAZAR"). Users may accidentally dismiss and get "Recomendación rechazada" logged.
**Why it happens:** MAUI uses native platform dialogs. Android's default behavior allows outside-tap dismissal.
**How to avoid:** This is acceptable behavior per CONTEXT.md — "No hay consecuencia funcional al aceptar o rechazar — es registro de decisión." Document in code comments. No code fix needed.
**Warning signs:** If users report many "rechazada" entries in event log, consider this cause.

### Pitfall 2: Popup Stacking on Rapid Rhythm Changes
**What goes wrong:** If user rapidly changes rhythm (e.g., TV → FV → TV), each change fires PropertyChanged and triggers a DisplayAlert. These stack — multiple alerts queue up.
**Why it happens:** DisplayAlert is `await`ed but the PropertyChanged handler fires synchronously for each change. Fire-and-forget pattern means each call starts independently.
**How to avoid:** Add a `_isPopupShowing` guard flag. Set to `true` before `await DisplayAlert`, `false` after. Skip showing new popup if already showing. This is important — rapid rhythm changes during real emergencies are plausible.
**Warning signs:** Test by rapidly tapping different rhythm buttons.

```csharp
private bool _isPopupShowing;

private async Task HandleRhythmChangeAsync(CardiacRhythm newRhythm)
{
    if (_isPopupShowing) return; // Prevent stacking
    _isPopupShowing = true;
    try
    {
        // ... popup logic ...
    }
    finally
    {
        _isPopupShowing = false;
    }
}
```

### Pitfall 3: Race Between OnPulseCheckDue and Rhythm Change
**What goes wrong:** Pulse check popup is showing (awaiting user response). User changes rhythm. Rhythm change popup tries to show but can't because pulse check popup is blocking the UI.
**Why it happens:** Both use `DisplayAlert` on the same Page. MAUI queues popups — the rhythm popup will show AFTER the pulse check popup is dismissed.
**How to avoid:** This is actually MAUI's default behavior — popups queue. It's acceptable because the user can only interact with one popup at a time. However, the rhythm popup may show stale information. Consider: during pulse check, the timer is already stopped, so this is a natural pause point. Acceptable as-is.
**Warning signs:** If popup queue grows long during testing, add the `_isPopupShowing` guard.

### Pitfall 4: Cycle Count Timing — When to Check
**What goes wrong:** IV/IO reminder should show on "primer ciclo" but the cycle counter increments at different points depending on flow.
**Why it happens:** `OnPulseCheckDue` fires first (at 2 min mark), then user presses DEFIBRILAR or CONTINUAR, which may trigger `NewCycle`. The counter should be checked BEFORE incrementing.
**How to avoid:** Check `_cycleCount == 0` in OnPulseCheckDue (before any increment). Increment `_cycleCount` only in `NewCycle()`. The first OnPulseCheckDue fires before any NewCycle call, so `_cycleCount` is still 0 — this is the correct "first cycle" check.

### Pitfall 5: H's and T's Access from MainViewModel
**What goes wrong:** `HsAndTsItems` is an ObservableCollection on EventRecordingViewModel. MainViewModel accesses `EventRecording.HsAndTsItems` directly.
**Why it happens:** This works fine — MainViewModel already holds a reference to EventRecordingViewModel (line 17).
**How to avoid:** Use LINQ `.Where(i => !i.IsChecked && !i.IsDismissed)` with `.ToList()` to snapshot. Don't iterate the ObservableCollection directly in the async context (it could change between enumeration points).
**Warning signs:** If items appear/disappear during popup display.

## Code Examples

### Complete OnPulseCheckDue Extension (Verified Against Existing Lines 119-169)

```csharp
// Source: Official MAUI docs + existing codebase pattern
private async void OnPulseCheckDue(object? sender, EventArgs e)
{
    _pulseCheckTimer?.Stop();

    var suggestions = new List<string>();

    // === PROTOCOL GUIDANCE (NEW - before medication suggestions) ===

    // IV/IO access — first cycle only
    if (_cycleCount == 0)
    {
        suggestions.Add("¿Colocó acceso IV/IO?");
    }

    // Compressor rotation — every pulse check
    suggestions.Add("¿Rotar compresor?");

    // H's and T's pending review
    var pendingHsTs = EventRecording.HsAndTsItems
        .Where(i => !i.IsChecked && !i.IsDismissed)
        .Select(i => i.Name)
        .ToList();
    if (pendingHsTs.Count > 0)
    {
        suggestions.Add($"Revisar H's y T's pendientes: {string.Join(", ", pendingHsTs)}");
    }

    // === EXISTING MEDICATION SUGGESTIONS (no changes) ===

    if (Timer.Timers[3].IsOverThreshold)
    {
        suggestions.Add("¿Hora de Adrenalina?");
    }

    if (Timer.Timers[4].IsOverThreshold &&
        EventRecording.CurrentRhythm is CardiacRhythm.TV or CardiacRhythm.FV &&
        _amiodaronaDoseCount < 2)
    {
        string doseHint = _amiodaronaDoseCount == 0
            ? "¿Hora de Amiodarona? (1ra dosis: 300mg)"
            : "¿Hora de Amiodarona? (2da dosis: 150mg)";
        suggestions.Add(doseHint);
    }

    // Build message
    string message = "Han pasado 2 minutos.\nConstate pulso y ritmo.\nAdministre 2 ventilaciones.";

    if (suggestions.Count > 0)
    {
        message += "\n\n" + string.Join("\n", suggestions);
    }

    // No ACEPTAR/RECHAZAR here — pulse check uses DEFIBRILAR/CONTINUAR (existing pattern)
    bool defibrilar = await Application.Current!.MainPage!
        .DisplayAlert("Check de Pulso", message, "DEFIBRILAR", "CONTINUAR");

    if (defibrilar)
    {
        EventRecording.LogCustomEventCommand.Execute("Defibrilación realizada");
    }
    else
    {
        Timer.PauseCompressions();
        Timer.StartPulseCheckTimer();
    }
}
```

### Complete Rhythm Change Handler (Replaces Lines 33-46)

```csharp
// Source: Existing pattern + MAUI DisplayAlert API docs
EventRecording.PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(EventRecordingViewModel.CurrentRhythm))
    {
        var rhythm = EventRecording.CurrentRhythm;
        IsAmiodaronaEnabled = rhythm is CardiacRhythm.TV or CardiacRhythm.FV;

        // Fire-and-forget async popup for rhythm-specific protocol guidance
        _ = HandleRhythmChangeAsync(rhythm);
    }
};
```

### DisplayAlert Two-Button Return Value (Verified: Official MAUI Docs)

```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups
// bool answer = await DisplayAlert("Question?", "Message", "Yes", "No");
// Returns: true = first button ("Yes"), false = second button ("No") or dismissed
//
// IMPORTANT: On Android, tapping outside dismisses = returns false
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| DisplayAlert | DisplayAlertAsync | MAUI 10.0 | Not applicable — project uses .NET 9 / MAUI 9. `DisplayAlert` is correct. |

**Note:** The project targets `net9.0` (MAUI 9). The `DisplayAlert` method is the correct API. In MAUI 10 it was renamed to `DisplayAlertAsync` but that's a future concern.

## Open Questions

1. **Should the pulse check popup also use ACEPTAR/RECHAZAR for the protocol suggestions?**
   - What we know: CONTEXT.md says pulse check uses existing DEFIBRILAR/CONTINUAR buttons. Protocol suggestions in the pulse check popup are informational — no accept/reject per se.
   - What's clear: The DEFIBRILAR/CONTINUAR response covers the clinical action. Protocol suggestions are FYI. No change needed.
   - Recommendation: Keep existing DEFIBRILAR/CONTINUAR for pulse check popup. ACEPTAR/RECHAZAR only for rhythm change popups.

2. **Should `_cycleCount` increment in OnPulseCheckDue OR only in NewCycle?**
   - What we know: CONTEXT.md says "se incrementa en NewCycle() o OnPulseCheckDue()". But the flow is: OnPulseCheckDue fires → user presses DEFIBRILAR or CONTINUAR → NewCycle may or may not be called.
   - Recommendation: Increment ONLY in NewCycle(). The OnPulseCheckDue handler already fires at the right time for the first check (before any NewCycle). After the first check, if user goes through DEFIBRILAR → NewCycle, the counter becomes 1. The IV/IO check at `_cycleCount == 0` correctly targets only the first pulse check ever.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — requires setup |
| Config file | None |
| Quick run command | N/A |
| Full suite command | N/A |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| REGI-04 | Pulse check popup includes IV/IO reminder on first cycle | Manual | N/A — UI popup verification | ❌ Wave 0 |
| REGI-04 | Pulse check popup includes "Rotar compresor" every cycle | Manual | N/A | ❌ Wave 0 |
| REGI-04 | Pulse check popup includes pending H's and T's list | Manual | N/A | ❌ Wave 0 |
| REGI-04 | Rhythm change to AESP/Asistolia shows reversible causes popup | Manual | N/A | ❌ Wave 0 |
| REGI-04 | Rhythm change to TV/FV shows defibrillation + H's and T's popup | Manual | N/A | ❌ Wave 0 |
| REGI-04 | Rhythm change to RCE shows post-ROSC checklist popup | Manual | N/A | ❌ Wave 0 |
| REGI-04 | ACEPTAR logs "Recomendación aceptada" to event log | Manual | N/A | ❌ Wave 0 |
| REGI-04 | RECHAZAR logs "Recomendación rechazada" to event log | Manual | N/A | ❌ Wave 0 |
| REGI-04 | Cycle counter resets on StartCode | Unit-testable | Manual verification | ❌ Wave 0 |
| REGI-04 | Popup stacking prevented on rapid rhythm changes | Manual | N/A | ❌ Wave 0 |

**Note:** All behaviors are DisplayAlert UI popups — inherently manual to verify on device. Unit testing would require mocking `Application.Current.MainPage.DisplayAlert` which introduces significant test infrastructure complexity for minimal ROI on a single-file change.

### Wave 0 Gaps
- All REGI-04 behaviors require on-device manual testing (DisplayAlert popups)
- No test framework installed — recommend manual testing only for this phase
- Logic that COULD be unit tested (cycle counter, suggestion builder) is tightly coupled to MAUI Application.Current — extraction for testability is over-engineering for this scope

**Recommendation:** Manual testing only. The changes are 100% additive UI popups in a single ViewModel file. Test plan: start code → wait 2 min → verify suggestions → change rhythms → verify popups → verify event log entries.

## Sources

### Primary (HIGH confidence)
- Official MAUI Pop-ups documentation: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups — DisplayAlert two-button API verified, returns `bool`
- Existing codebase MainViewModel.cs (191 lines) — all integration points verified in source
- Existing codebase EventRecordingViewModel.cs (116 lines) — CurrentRhythm, HsAndTsItems, LogCustomEventCommand verified

### Secondary (MEDIUM confidence)
- MAUI versioning: project uses .NET 9 / MAUI 9 (`net9.0-*` target frameworks in .csproj) — DisplayAlert is correct API name (not DisplayAlertAsync which is MAUI 10)

### Tertiary (LOW confidence)
- None — all findings verified against official docs and source code

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all dependencies already in project, verified in .csproj
- Architecture: HIGH — integration points verified line-by-line in existing codebase
- Pitfalls: HIGH — DisplayAlert behavior verified against official docs, Android dismiss behavior documented
- Protocol rules: HIGH — AHA ACLS 2020 rules are well-established medical protocol, rules specified verbatim in CONTEXT.md

**Research date:** 29/03/2026
**Valid until:** 30 days — stable .NET MAUI 9 API, no fast-moving dependencies
