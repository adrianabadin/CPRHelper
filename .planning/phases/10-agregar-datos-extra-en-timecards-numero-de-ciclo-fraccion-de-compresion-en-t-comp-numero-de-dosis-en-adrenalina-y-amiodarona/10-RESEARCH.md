# Phase 10: Agregar datos extra en timecards - Research

**Researched:** 2026-04-11
**Domain:** .NET MAUI MVVM data binding + XAML UI enhancement
**Confidence:** HIGH

## Summary

This phase adds secondary informational text to existing TimerCard controls without changing card height or layout structure. The core mechanism is straightforward: add an `[ObservableProperty]` string `ExtraInfo` to `TimerModel`, bind it to a new subtle Label in `TimerCard.xaml` Row 1, and update it from `MainViewModel` at the appropriate integration points (NewCycle, Adrenalina/Amiodarona commands, timer ticks for FCT, ResetCodeState).

The existing codebase uses CommunityToolkit.Mvvm source generators extensively (`[ObservableProperty]`, `[RelayCommand]`), and the TimerCard binding context is already set to individual `TimerModel` instances. Adding a property to `TimerModel` and binding it from XAML is the natural pattern. The main subtlety is FCT (compression fraction) calculation, which requires accumulating compression time vs total code time — but the compressions timer already accumulates (intentionally not reset between cycles per `NewCprCycle()`), so `Timers[2].Elapsed` divided by `Timers[0].Elapsed` gives FCT.

**Primary recommendation:** Add `[ObservableProperty] string ExtraInfo` and `[ObservableProperty] bool IsExtraInfoVisible` to `TimerModel`. In `TimerCard.xaml`, add a horizontal StackLayout in Row 1 Column 0 containing the existing time Label + a new subtle Label for ExtraInfo. Update from MainViewModel at each integration point.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Dato a la derecha del tiempo transcurrido (mm:ss), en la misma fila existente (Row 1 del Grid actual)
- NO se agregan filas nuevas ni se aumenta la altura del TimerCard
- El dato se oculta (IsVisible=false) cuando no hay valor (valor = 0 o no aplica)
- Timers sin dato extra: Tiempo Total y T. Pulsos
- FontSize: 12pt, FontAttributes: Normal (no bold)
- Color: gris #999999 (mismo color en light y dark mode)
- Todos los datos extra comparten exactamente el mismo estilo
- El tiempo transcurrido sigue siendo 20pt Bold #111111/#FFFFFF
- **Ciclo RCP** (Timers[1]): número de ciclo entero (solo el número, sin prefijo)
- **T.Comp** (Timers[2]): fracción de compresión torácica como porcentaje (solo el número + "%", ej: "67%")
- **Adrenalina** (Timers[3]): número de dosis entero (solo el número, sin prefijo)
- **Amiodarona** (Timers[4]): número de dosis entero (solo el número, sin prefijo)
- **Tiempo Total** (Timers[0]): sin dato extra
- **T. Pulsos** (Timers[5]): sin dato extra
- Nuevo contador acumulativo `_adrenalinaDoseCount` en MainViewModel (igual que `_amiodaronaDoseCount`)
- Se incrementa cada vez que se ejecuta el comando Adrenalina
- Se resetea solo en NUEVO CODIGO (ResetCodeState), NO al CONTINUAR
- Consistente con el comportamiento existente de amiodarona
- Todos los datos extra se ocultan cuando valor = 0
- Ciclo: oculto cuando _cycleCount = 0 (aparece "1" después del primer NewCycle)
- Adrenalina: oculto cuando _adrenalinaDoseCount = 0 (aparece "1" después de primera dosis)
- Amiodarona: oculto cuando _amiodaronaDoseCount = 0 (aparece "1" después de primera dosis)
- FCT: siempre visible cuando el timer de compresiones está corriendo (mostrar 0% es válido)

### Claude's Discretion
- Cómo pasar los datos del MainViewModel al TimerModel para binding (BindableProperty, propiedad directa en TimerModel, o converter)
- Cómo calcular la FCT: (tiempo de compresiones acumulado / tiempo total transcurrido) * 100
- Formato exacto del FCT% (¿0% o 0.0%?)
- Layout XAML exacto para acomodar el dato a la derecha del tiempo sin desbordar

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| ENH-10 | Agregar número de ciclo en TimerCard de Ciclo RCP | TimerModel.ExtraInfo property + MainViewModel.NewCycle() integration point |
| ENH-10 | Agregar FCT% en TimerCard de T.Comp | TimerModel.ExtraInfo property + timer tick update for FCT calculation |
| ENH-10 | Agregar número de dosis en TimerCard de Adrenalina | New `_adrenalinaDoseCount` field + MainViewModel.Adrenalina() integration point |
| ENH-10 | Agregar número de dosis en TimerCard de Amiodarona | Existing `_amiodaronaDoseCount` field + MainViewModel.Amiodarona() integration point |
| ENH-10 | Estilo visual sutil (12pt, gris #999999) para datos extra | XAML Label with fixed style in TimerCard.xaml |
| ENH-10 | Visibilidad condicional (ocultar en 0) | IsExtraInfoVisible bool property on TimerModel |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CommunityToolkit.Mvvm | Current (already in project) | `[ObservableProperty]` source generator for ExtraInfo | Already used for all TimerModel properties and ViewModel fields |
| .NET MAUI | .NET 8+ (already in project) | XAML binding, DataTrigger for IsVisible | Already the project framework |
| CommunityToolkit.Maui | Current (already in project) | UI performance optimizations | Already in project for converters and animations |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| None needed | N/A | N/A | All required tools already exist in project |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| ExtraInfo string on TimerModel | BindableProperty on TimerCard | ExtraInfo on model is simpler — model already has BindingContext, no need for additional BindableProperty wiring. BindableProperty adds complexity for no benefit since BindingContext IS the TimerModel. |
| Horizontal StackLayout for Row 1 | Grid with 3 columns | StackLayout simpler for 2-item horizontal arrangement. Grid would need column definition changes in TimerCard. |

**Installation:**
None required — all dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
No structural changes needed. Changes are confined to existing files:
```
AclsTracker/
├── Models/
│   └── TimerModel.cs           # ADD: ExtraInfo, IsExtraInfoVisible properties
├── Controls/
│   └── TimerCard.xaml          # MODIFY: Add subtle Label for ExtraInfo
├── ViewModels/
│   └── MainViewModel.cs        # MODIFY: Add _adrenalinaDoseCount, update ExtraInfo at all integration points
└── Services/Timer/
    └── TimerService.cs         # NO CHANGE (FCT calculated from existing Elapsed values)
```

### Pattern 1: ObservableProperty on TimerModel for UI Binding
**What:** Add `[ObservableProperty] private string _extraInfo = string.Empty;` to `TimerModel`. The CommunityToolkit.Mvvm source generator creates `ExtraInfo` property with INotifyPropertyChanged support. TimerCard.xaml binds directly to `{Binding ExtraInfo}`.
**When to use:** This is the established pattern in the project — all TimerModel properties (`Elapsed`, `IsRunning`, `IsPaused`) use this pattern.
**Example:**
```csharp
// In TimerModel.cs
[ObservableProperty]
private string _extraInfo = string.Empty;

[ObservableProperty]
private bool _isExtraInfoVisible;
```

### Pattern 2: MainViewModel Updates TimerModel Directly
**What:** MainViewModel already accesses timers by index (`Timer.Timers[N]`) and sets properties directly. The same pattern applies for `ExtraInfo`.
**When to use:** Every integration point where state changes.
**Example:**
```csharp
// In MainViewModel.NewCycle()
_cycleCount++;
Timer.Timers[1].ExtraInfo = _cycleCount.ToString();
Timer.Timers[1].IsExtraInfoVisible = _cycleCount > 0;

// In MainViewModel.Adrenalina()
_adrenalinaDoseCount++;
Timer.Timers[3].ExtraInfo = _adrenalinaDoseCount.ToString();
Timer.Timers[3].IsExtraInfoVisible = _adrenalinaDoseCount > 0;
```

### Pattern 3: FCT Calculation from Existing Timer Values
**What:** FCT = (compression time / total time) * 100. Both values already exist as `Timers[2].Elapsed` and `Timers[0].Elapsed`. The compressions timer accumulates across cycles (intentionally NOT reset in `NewCprCycle()`), and the total timer runs continuously.
**When to use:** Updated on each timer tick (~20Hz) for real-time FCT display.
**Example:**
```csharp
// In MainViewModel or TimerService.UpdateTimerValues
private void UpdateCompressionFraction()
{
    var totalElapsed = Timer.Timers[0].Elapsed;
    var compressionElapsed = Timer.Timers[2].Elapsed;

    if (totalElapsed.TotalSeconds > 0)
    {
        double fct = (compressionElapsed.TotalSeconds / totalElapsed.TotalSeconds) * 100;
        Timer.Timers[2].ExtraInfo = $"{fct:F0}%";  // "67%" format
    }
    else
    {
        Timer.Timers[2].ExtraInfo = "0%";
    }

    Timer.Timers[2].IsExtraInfoVisible = Timer.Timers[2].IsRunning;
}
```

### Anti-Patterns to Avoid
- **BindableProperty on TimerCard for ExtraInfo:** Unnecessary indirection. TimerCard's BindingContext is already the TimerModel, so binding to `{Binding ExtraInfo}` works directly. Adding BindableProperty creates wiring complexity for zero benefit.
- **Converter for ExtraInfo visibility:** Don't create a StringToBoolConverter. Use the explicit `IsExtraInfoVisible` bool property on TimerModel for clear control.
- **Resetting compressions timer:** The compressions timer MUST continue accumulating across cycles. `NewCprCycle()` already has a comment: "Compressions timer intentionally NOT reset — accumulates total compression time for FCT." This is critical for correct FCT.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Observable property notification | Manual INotifyPropertyChanged | CommunityToolkit.Mvvm `[ObservableProperty]` | Already project standard, source-generated, zero boilerplate |
| Timer visibility logic | StringToBoolConverter | `[ObservableProperty] bool _isExtraInfoVisible` on TimerModel | Simpler, explicit, no converter registration needed |

**Key insight:** The project already has the perfect architecture for this feature. TimerModel is an ObservableObject with source-generated properties, and its instances are the BindingContext of TimerCard controls. Adding a property and binding to it is trivial.

## Common Pitfalls

### Pitfall 1: FCT Calculation Before Timer Start
**What goes wrong:** Division by zero when total elapsed is 0 (code hasn't started yet).
**Why it happens:** `Timers[0].Elapsed` is `TimeSpan.Zero` before StartCode.
**How to avoid:** Check `totalElapsed.TotalSeconds > 0` before dividing. Show "0%" when no time elapsed.
**Warning signs:** DivideByZero exception or NaN display.

### Pitfall 2: ExtraInfo Not Reset on New Code
**What goes wrong:** After NUEVO CODIGO, old cycle/dose counts still visible.
**Why it happens:** `ResetCodeState()` resets `_cycleCount` and `_amiodaronaDoseCount` but doesn't clear `TimerModel.ExtraInfo`.
**How to avoid:** In `ResetCodeState()`, clear ExtraInfo on all timers:
```csharp
foreach (var timer in Timer.Timers)
{
    timer.ExtraInfo = string.Empty;
    timer.IsExtraInfoVisible = false;
}
```
**Warning signs:** Stale data visible after restarting code.

### Pitfall 3: FCT Update Location — Don't Block TimerService
**What goes wrong:** Adding FCT calculation inside `TimerService.UpdateTimerValues()` adds coupling between service and view model logic.
**Why it happens:** UpdateTimerValues runs at 20Hz and seems like a convenient place.
**How to avoid:** FCT calculation belongs in MainViewModel. Subscribe to `Timers[2].PropertyChanged` (Elapsed changes) or use the same dispatcher timer approach. However, since UpdateTimerValues already iterates all timers and updates Elapsed at 20Hz, a lightweight FCT update there IS acceptable if done via a callback/event. Simplest approach: have MainViewModel subscribe to `Timer.Timers[0].PropertyChanged` for Elapsed changes and update FCT. BUT this creates 20Hz PropertyChanged events which is already happening — just hook into existing Elapsed change notifications.
**Warning signs:** Performance degradation or tight coupling between layers.

### Pitfall 4: Adrenalina Dose Count Not Persisted on CONTINUAR
**What goes wrong:** If `_adrenalinaDoseCount` is reset on CONTINUAR, the dose count will be wrong.
**Why it happens:** ResetCodeState() clears all counts, but CONTINUAR should NOT reset.
**How to avoid:** Only reset `_adrenalinaDoseCount` in `ResetCodeState()` (which is called for NUEVO CODIGO only). The existing `_hasCompletedCode` flag + "CONTINUAR/NUEVO CODIGO" dialog ensures ResetCodeState is NOT called when user continues.
**Warning signs:** Dose count resets to 0 after stopping and continuing a code.

### Pitfall 5: XAML Layout Overflow on Narrow Screens
**What goes wrong:** Time display (mm:ss) + extra info + pause button exceeds card width.
**Why it happens:** Row 1 currently has time (Column 0, `*` width) and pause button (Column 1, `Auto`). Adding extra info text in Column 0 may not fit.
**How to avoid:** Replace the single time Label with a horizontal StackLayout containing time + extra info, with the extra info Label having `HorizontalOptions="End"` and the time Label keeping `HorizontalOptions="Start"`. The `*` column definition will give it the full width minus the pause button. Alternative: keep the Grid with `ColumnDefinitions="*, Auto, Auto"` where time is Col 0, extra info is Col 1, pause is Col 2.
**Warning signs:** Text truncated or overlapping on small screens.

## Code Examples

### TimerModel.cs — Add Properties
```csharp
// ADD to TimerModel.cs after existing [ObservableProperty] fields

/// <summary>
/// Secondary informational text displayed subtly next to elapsed time.
/// e.g., cycle number, FCT%, dose count. Empty string = not shown.
/// </summary>
[ObservableProperty]
private string _extraInfo = string.Empty;

/// <summary>
/// Controls visibility of the ExtraInfo label. Set to false to hide.
/// </summary>
[ObservableProperty]
private bool _isExtraInfoVisible;
```

### TimerCard.xaml — Add ExtraInfo Label (Recommended: Horizontal StackLayout approach)
```xml
<!-- REPLACE the elapsed Label in Row 1, Column 0 with: -->
<HorizontalStackLayout Grid.Row="1" Grid.Column="0"
                       HorizontalOptions="FillAndExpand"
                       Spacing="8">
    <!-- Digital elapsed display -->
    <Label Text="{Binding Elapsed, StringFormat='{0:mm\\:ss}'}"
           FontSize="20"
           FontAttributes="Bold"
           FontFamily="OpenSansRegular"
           TextColor="{AppThemeBinding Light=#111111, Dark=#FFFFFF}">
        <Label.Triggers>
            <DataTrigger TargetType="Label"
                         Binding="{Binding IsOverThreshold}"
                         Value="True">
                <Setter Property="TextColor" Value="#D32F2F" />
            </DataTrigger>
        </Label.Triggers>
    </Label>

    <!-- Extra info label (subtle, secondary) -->
    <Label Text="{Binding ExtraInfo}"
           FontSize="12"
           FontAttributes="Normal"
           TextColor="#999999"
           VerticalOptions="Center"
           IsVisible="{Binding IsExtraInfoVisible}" />
</HorizontalStackLayout>
```

### MainViewModel.cs — Integration Points

#### New Field
```csharp
// Add alongside existing _amiodaronaDoseCount
private int _adrenalinaDoseCount;
```

#### NewCycle() — Update Cycle ExtraInfo
```csharp
[RelayCommand]
private void NewCycle()
{
    _cycleCount++;
    Timer.Timers[1].ExtraInfo = _cycleCount.ToString();
    Timer.Timers[1].IsExtraInfoVisible = true;
    // ... rest of existing code
}
```

#### Adrenalina() — Increment Dose Count and Update ExtraInfo
```csharp
[RelayCommand]
private void Adrenalina()
{
    _adrenalinaDoseCount++;
    _lastAdrenalinaTime = DateTime.Now;
    Timer.Timers[3].ExtraInfo = _adrenalinaDoseCount.ToString();
    Timer.Timers[3].IsExtraInfoVisible = true;
    Timer.MarkMedicationGivenCommand.Execute(null);
    EventRecording.LogCustomEventCommand.Execute("Adrenalina administrada");
    _adrenalinaBannerFired = false;
    UpdateDrugSuggestions();
}
```

#### Amiodarona() — Update ExtraInfo (doses already counted)
```csharp
[RelayCommand]
private void Amiodarona()
{
    _amiodaronaDoseCount++;
    _lastAmiodaronaTime = DateTime.Now;
    Timer.Timers[4].ExtraInfo = _amiodaronaDoseCount.ToString();
    Timer.Timers[4].IsExtraInfoVisible = true;
    Timer.MarkAmiodaronaGivenCommand.Execute(null);
    EventRecording.LogCustomEventCommand.Execute("Amiodarona administrada");
    _amiodaronaBannerFired = false;
    UpdateDrugSuggestions();
}
```

#### ResetCodeState() — Clear All ExtraInfo
```csharp
private void ResetCodeState()
{
    _cycleCount = 0;
    _amiodaronaDoseCount = 0;
    _adrenalinaDoseCount = 0;  // NEW
    // ... existing resets ...

    // Clear ExtraInfo on all timers
    foreach (var timer in Timer.Timers)
    {
        timer.ExtraInfo = string.Empty;
        timer.IsExtraInfoVisible = false;
    }

    Timer.ResetAllCommand.Execute(null);
    // ... rest of existing code ...
}
```

#### FCT Update — Subscribe to Elapsed Changes
```csharp
// In MainViewModel constructor, add after existing subscriptions:
Timer.Timers[0].PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(TimerModel.Elapsed))
    {
        UpdateCompressionFraction();
    }
};

Timer.Timers[2].PropertyChanged += (_, e) =>
{
    if (e.PropertyName == nameof(TimerModel.IsRunning))
    {
        UpdateCompressionFraction();
    }
};

private void UpdateCompressionFraction()
{
    var totalElapsed = Timer.Timers[0].Elapsed;
    var compElapsed = Timer.Timers[2].Elapsed;

    if (totalElapsed.TotalSeconds > 0 && Timer.Timers[2].IsRunning)
    {
        double fct = (compElapsed.TotalSeconds / totalElapsed.TotalSeconds) * 100;
        Timer.Timers[2].ExtraInfo = $"{fct:F0}%";
        Timer.Timers[2].IsExtraInfoVisible = true;
    }
    else if (Timer.Timers[2].IsRunning)
    {
        Timer.Timers[2].ExtraInfo = "0%";
        Timer.Timers[2].IsExtraInfoVisible = true;
    }
    else
    {
        Timer.Timers[2].ExtraInfo = string.Empty;
        Timer.Timers[2].IsExtraInfoVisible = false;
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual INotifyPropertyChanged | CommunityToolkit.Mvvm `[ObservableProperty]` | Project inception | Zero boilerplate for this feature |
| BindableProperty per control | Direct model binding | Project inception | Simpler binding for ExtraInfo |

**Deprecated/outdated:**
- None relevant — the project uses current patterns throughout.

## Open Questions

1. **FCT Format: Integer or Decimal?**
   - What we know: User said "solo el número + %". FCT ranges 0-100.
   - Recommendation: Use integer format `"{fct:F0}%"` → "67%". Decimal precision (67.3%) is unnecessary for real-time display during a code and harder to read quickly on a small screen.

2. **FCT Update Frequency: 20Hz Performance**
   - What we know: PropertyChanged fires for Elapsed at ~20Hz (50ms). Subscribing to calculate FCT means string formatting at 20Hz.
   - What's unclear: Whether this has any perceptible performance impact on mobile.
   - Recommendation: Negligible — it's a simple division and string format. The existing timer update already does this for 6 timers at 20Hz. One more string property is trivial.

3. **FCT Visibility When Paused**
   - What we know: User said "FCT siempre visible cuando el timer de compresiones está corriendo".
   - Recommendation: When compressions are paused (pulse check), hide FCT. When resumed, show again with current value. This is cleanest and avoids showing stale percentages during non-compression periods.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected in project |
| Config file | None |
| Quick run command | N/A |
| Full suite command | N/A |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| ENH-10 | ExtraInfo property on TimerModel notifies on change | unit | N/A — manual verification | ❌ |
| ENH-10 | FCT calculates correctly (compression/total * 100) | unit | N/A — manual verification | ❌ |
| ENH-10 | Adrenalina dose count increments and displays | unit | N/A — manual verification | ❌ |
| ENH-10 | Amiodarona dose count increments and displays | unit | N/A — manual verification | ❌ |
| ENH-10 | ExtraInfo hides when value is 0 | visual | Device/emulator test | ❌ |
| ENH-10 | ExtraInfo clears on NUEVO CODIGO | unit | N/A — manual verification | ❌ |
| ENH-10 | ExtraInfo persists on CONTINUAR | unit | N/A — manual verification | ❌ |

### Sampling Rate
- **Per task commit:** Build + visual verification on emulator
- **Per wave merge:** Full manual walkthrough (start code → cycle → drugs → FCT → stop → continue → stop → new code)
- **Phase gate:** All 4 extra info displays verified on device

### Wave 0 Gaps
- No test framework in project — all verification is manual/device testing
- No unit test files exist — feature is small enough for manual verification

## Sources

### Primary (HIGH confidence)
- Source code analysis: `TimerModel.cs`, `TimerCard.xaml`, `TimerCard.xaml.cs`, `MainViewModel.cs`, `TimerViewModel.cs`, `TimerService.cs`, `MainPage.xaml`
- CommunityToolkit.Mvvm `[ObservableProperty]` pattern — verified in existing project code

### Secondary (MEDIUM confidence)
- CONTEXT.md decisions — verified against actual code structure

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all dependencies already in project, no new libraries needed
- Architecture: HIGH — direct code analysis of all 7 files involved, patterns are clear and established
- Pitfalls: HIGH — identified from actual code flow analysis (division by zero, reset on continue, layout overflow)
- XAML layout: MEDIUM — need device testing to confirm no overflow on narrow screens

**Research date:** 2026-04-11
**Valid until:** 2026-05-11 (stable — no external dependency changes)
