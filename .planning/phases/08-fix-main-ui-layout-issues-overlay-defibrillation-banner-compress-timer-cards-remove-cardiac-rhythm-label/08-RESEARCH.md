# Phase 08: Fix Main UI Layout Issues - Research

**Researched:** 2026-04-05
**Domain:** .NET MAUI XAML layout — Grid overlay, ContentView restructuring, DataTrigger binding
**Confidence:** HIGH

## Summary

This phase is three independent, surgical UI changes to MainPage and two controls (TimerCard, RhythmSelector). All changes are purely cosmetic/structural — no new services, commands, or data flows are introduced. The codebase is well-understood from prior phases, so research is focused on confirmed MAUI patterns for each specific change.

**Change 1 — Banner overlay:** MainPage currently uses `RowDefinitions="Auto,*"` with the banner in Row 0 (Auto) pushing content down. The fix is to collapse the grid to a single row and let both the banner and the ScrollView occupy the same row. With MAUI Grid, siblings in the same cell overlap; the higher ZIndex element appears on top. The banner already has `ZIndex="10"` set on its root element.

**Change 2 — TimerCard compression:** The control has a 4-row Grid (`Auto, Auto, Auto, Auto`). Row 2 holds the "EN PAUSA" label and the pause button side by side. The plan is to remove Row 2 entirely, collapse to 3 rows, and move the pause button into Row 1 alongside the elapsed time label, right-aligned. The "EN PAUSA" label is removed entirely. The timer name is changed from "Compresiones" to "T.Comp" at the model initialization site in `TimerViewModel.InitializeDefaultTimers()`.

**Change 3 — Rhythm selector:** Remove the static `<Label Text="RITMO CARDÍACO" ...>` from `RhythmSelector.xaml`. Add an active-button highlight using a `DataTrigger` on each Button's `BorderColor` and `BorderWidth`, binding to a new per-button IsActive computed property exposed from `EventRecordingViewModel`, or using a `CommandParameter`-matching converter pattern already established in the project.

**Primary recommendation:** Execute all three changes in sequence — they are independent, low-risk, and localized to exactly 3 XAML files + 1 ViewModel line.

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Banner overlay behavior**
- Move NotificationBanner from its own Grid.Row="0" (Auto) to overlay on top of ScrollView content
- Position: top of the content area, floating over INICIAR CODIGO and whatever is below
- Animation: slide down from top + fade in on appear, reverse on dismiss
- Background: fully opaque (change from current 0.95 opacity to 1.0)
- Auto-dismiss: keep existing 8-second timeout, user can also tap ✕
- ZIndex already set to 10 — ensure it stays above all content
- Implementation: put banner and ScrollView in the same Grid row, banner with VerticalOptions="Start"

**Timer card — compressions (T.Comp)**
- Rename compressions timer display name to "T.Comp" (Claude decides where — model init or ViewModel)
- Remove "EN PAUSA" label entirely (Row 2, Column 0 in TimerCard.xaml)
- Move pause/resume button from Row 2 to Row 1, right-aligned next to the elapsed time (00:00)
- This change applies ONLY to compressions timer card (Timers[2], ShowPauseButton=True)
- All 6 timer cards should have uniform height — standardize to 3-row layout: Name, Elapsed+icon, Progress bar
- Remove Row 2 from TimerCard Grid RowDefinitions (was "Auto, Auto, Auto, Auto" → "Auto, Auto, Auto")

**Rhythm selector labels**
- Remove "RITMO CARDÍACO" section header label (line 10-14 of RhythmSelector.xaml)
- Keep "Ritmo actual: {0}" subtitle — user needs to see current rhythm name
- Add white border highlight on the currently active rhythm button (DataTrigger or binding-based)
- Non-selected buttons keep their current appearance (no border or transparent border)

### Claude's Discretion
- Exact animation duration/easing for banner slide-in
- Where to change the "T.Comp" name (model Name property at init vs display override)
- Border thickness and style for active rhythm button highlight
- Any minor spacing adjustments needed after removing elements

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

---

## Standard Stack

### Core (already in use — no new dependencies)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET MAUI | .NET 8+ | XAML Grid layout, ContentView, DataTrigger | Project framework |
| CommunityToolkit.Mvvm | Latest | ObservableProperty, RelayCommand | Already used in all ViewModels |
| CommunityToolkit.Maui | Latest | Animation extensions (FadeTo, TranslateTo) | Already registered in MauiProgram |

### No New Packages Required
All three changes use existing MAUI primitives: `Grid`, `DataTrigger`, `IsVisible`, `VerticalOptions`, `ZIndex`, `BorderColor`, `BorderWidth`.

---

## Architecture Patterns

### Pattern 1: MAUI Grid Overlay (same-row stacking)

**What:** Place two elements in the same Grid row/column. MAUI renders them layered by document order (later = on top) and by ZIndex.

**When to use:** Floating banners, overlays, badges — any element that should hover over content without consuming layout space.

**Example — single-row overlay grid:**
```xml
<!-- Change RowDefinitions="Auto,*" to just "*" -->
<Grid>
    <!-- ScrollView occupies full row -->
    <ScrollView>
        <!-- ... content ... -->
    </ScrollView>

    <!-- Banner sits in same row, sticks to top, floats over ScrollView -->
    <controls:NotificationBanner x:Name="BannerControl"
                                 VerticalOptions="Start"
                                 ZIndex="10" />
</Grid>
```

**Key detail:** Remove `Grid.Row="0"` and `Grid.Row="1"` attributes from both children when collapsing to a single-row Grid. Remove the `RowDefinitions` attribute entirely (default is single `*` row).

**Source:** MAUI Grid layout docs — overlapping in same cell is standard MAUI behavior. Confirmed in project: existing `Grid HorizontalOptions="Fill"` at MainPage line 33 already uses this pattern for INICIAR/FINALIZAR button overlap.

---

### Pattern 2: Banner Slide-Down + Fade Animation

**What:** Use `TranslateTo` + `FadeTo` in code-behind when `IsBannerVisible` changes.

**When to use:** Notification banners that need clear entry/exit motion.

**Current state:** `NotificationBanner.xaml.cs` `Show()` method sets `IsBannerVisible = true` directly — no animation. `HideBanner()` sets it to false directly.

**Recommended implementation** (discretion area — animate in/out from `Show()` and `HideBanner()`):

```csharp
// In NotificationBanner.xaml.cs
public async void Show(string message, int autoDismissSeconds = 5)
{
    BannerMessage = message;
    IsBannerVisible = true;

    // Slide from above + fade in
    BannerGrid.TranslationY = -BannerGrid.Height;
    BannerGrid.Opacity = 0;
    await Task.WhenAll(
        BannerGrid.TranslateTo(0, 0, 200, Easing.SinOut),
        BannerGrid.FadeTo(1.0, 150)
    );

    // ... timer setup unchanged ...
}

public async void HideBanner()
{
    await Task.WhenAll(
        BannerGrid.TranslateTo(0, -(BannerGrid.Height + 10), 180, Easing.SinIn),
        BannerGrid.FadeTo(0, 150)
    );
    IsBannerVisible = false;
    _dismissTimer?.Stop();
}
```

**Pitfall:** `BannerGrid.Height` is 0 before first render. Safe approach: translate by a fixed offset (e.g., -60) rather than measuring height. Alternatively, start at `Opacity=0` and `TranslationY=-60`, then animate to `Opacity=1` and `TranslationY=0`. The `x:Name="BannerGrid"` is already declared in the XAML.

**Animation guidance (discretion):** 200ms translate + 150ms fade — fast enough for emergency UI, visible enough to notice.

---

### Pattern 3: TimerCard Grid Restructure (3-row)

**What:** Collapse TimerCard from 4 rows to 3 rows. Move pause button from Row 2 to Row 1 (same row as elapsed time).

**Current layout:**
```
Row 0: Timer Name (Col 0) | Running indicator ● (Col 1)
Row 1: Elapsed time 00:00 (Col 0-1 span)
Row 2: "EN PAUSA" label (Col 0) | Pause button (Col 1)   ← REMOVE this row
Row 3: Progress bar (Col 0-1 span)
```

**Target layout:**
```
Row 0: Timer Name (Col 0) | Running indicator ● (Col 1)
Row 1: Elapsed time 00:00 (Col 0) | Pause button (Col 1)   ← pause button moved here
Row 2: Progress bar (Col 0-1 span)
```

**Key XAML changes:**
1. `RowDefinitions="Auto, Auto, Auto, Auto"` → `RowDefinitions="Auto, Auto, Auto"`
2. Elapsed time label: remove `Grid.ColumnSpan="2"` — it now only spans Col 0
3. Pause button: change `Grid.Row="2"` to `Grid.Row="1"`, `Grid.Column="1"` stays
4. Remove the "EN PAUSA" label block entirely (lines 56-64 of current TimerCard.xaml)
5. Progress bar: change `Grid.Row="3"` to `Grid.Row="2"`

**Result:** All 6 timer cards get uniform 3-row height — the EN PAUSA text removal eliminates height variation between the compressions card (which had Row 2 content) and the other 5 cards (which had empty Row 2).

---

### Pattern 4: Active Rhythm Button Highlight via DataTrigger

**What:** Each rhythm button needs a white border when it is the currently selected rhythm. The DataTrigger pattern is established throughout the codebase (IsOverThreshold, IsSuggested, IsRunning).

**Challenge:** The `RhythmSelector` binding context is `EventRecordingViewModel`. Each button's `CommandParameter` is a `CardiacRhythm` enum value. We need each button to know "am I the active one?"

**Recommended approach — add a converter:** Create `IsEqualConverter` (or `IsActiveRhythmConverter`) that takes `(CurrentRhythm, CommandParameter)` and returns `true` if equal. Then use a single `MultiBinding` on `BorderColor`.

However, MAUI MultiBinding on Triggers has limited support. The simpler approach that matches existing patterns:

**Option A — per-rhythm bool properties on EventRecordingViewModel (straightforward, matches project pattern):**

```csharp
// In EventRecordingViewModel.cs
public bool IsRhythmRCE      => CurrentRhythm == CardiacRhythm.RCE;
public bool IsRhythmAESP     => CurrentRhythm == CardiacRhythm.AESP;
public bool IsRhythmAsistolia=> CurrentRhythm == CardiacRhythm.Asistolia;
public bool IsRhythmTV       => CurrentRhythm == CardiacRhythm.TV;
public bool IsRhythmFV       => CurrentRhythm == CardiacRhythm.FV;

partial void OnCurrentRhythmChanged(CardiacRhythm value)
{
    OnPropertyChanged(nameof(CurrentRhythmDisplay));
    OnPropertyChanged(nameof(IsRhythmRCE));
    OnPropertyChanged(nameof(IsRhythmAESP));
    OnPropertyChanged(nameof(IsRhythmAsistolia));
    OnPropertyChanged(nameof(IsRhythmTV));
    OnPropertyChanged(nameof(IsRhythmFV));
}
```

Then in RhythmSelector.xaml, each button gets a DataTrigger:

```xml
<Button ...>
    <Button.Triggers>
        <DataTrigger TargetType="Button"
                     Binding="{Binding IsRhythmRCE}"
                     Value="True">
            <Setter Property="BorderColor" Value="White" />
            <Setter Property="BorderWidth" Value="2" />
        </DataTrigger>
    </Button.Triggers>
</Button>
```

**Option B — value converter comparing enum to parameter:** More elegant but requires a new `IsEqualConverter`. Not needed given Option A's simplicity.

**Recommended: Option A.** It exactly matches the `IsSuggested` / `IsOverThreshold` pattern already used for drug buttons and timer cards. No new converter needed.

**Border style guidance (discretion):** `BorderWidth="2"` and `BorderColor="White"` — visible on all button background colors (green, orange, red). Non-selected buttons: `BorderWidth="0"` or `BorderColor="Transparent"` (MAUI defaults — no change needed since buttons have no border by default).

---

### Pattern 5: Timer Name Change Location

**Discretion area:** Change `"Compresiones"` to `"T.Comp"` in `TimerViewModel.InitializeDefaultTimers()` at line 47:

```csharp
// Before
_timerService.AddTimer("compressions", "Compresiones", TimerType.Compressions, null);

// After
_timerService.AddTimer("compressions", "T.Comp", TimerType.Compressions, null);
```

This is the correct location: the `TimerModel.Name` property is what binds to `TimerCard`'s `<Label Text="{Binding Name}">` in Row 0. No display override layer exists — the Name is the display text.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Overlay floating element | New AbsoluteLayout wrapper or custom renderer | Grid same-cell stacking + ZIndex | MAUI Grid already handles overlap natively |
| Active-state button style | Custom control or style inheritance tree | DataTrigger + BorderColor/BorderWidth | Exact same pattern as drug button suggestions (lines 111-117 in MainPage.xaml) |
| Banner animation | Platform-specific UIView animation | CommunityToolkit.Maui `FadeTo` + `TranslateTo` | Cross-platform, already available |
| Rhythm IsActive binding | MultiBinding or attached behavior | 5 bool computed properties on ViewModel | Follows IsSuggested/IsOverThreshold precedent |

---

## Common Pitfalls

### Pitfall 1: Forgetting Grid.Row attributes when collapsing rows

**What goes wrong:** After removing `RowDefinitions="Auto,*"` from MainPage and adding both children to the same single row, if `Grid.Row="0"` or `Grid.Row="1"` remain on children, MAUI will create implicit rows and the layout breaks back to stacked behavior.

**How to avoid:** Remove ALL `Grid.Row` attributes from NotificationBanner and ScrollView in MainPage when switching to single-row grid. Also remove `RowDefinitions` attribute entirely (default is `*`).

**Warning signs:** Banner still takes up space at top, content is pushed down.

---

### Pitfall 2: TimerCard Row 2 → Row 1 index shift

**What goes wrong:** Moving the pause button from `Grid.Row="2"` to `Grid.Row="1"` is correct, but forgetting to also update the progress bar from `Grid.Row="3"` to `Grid.Row="2"` leaves it outside the 3-row grid (MAUI will auto-create a 4th row anyway — the bar appears but layout is inconsistent).

**How to avoid:** After removing Row 2 definition, systematically update ALL row assignments: elapsed=Row1, pause=Row1, progressbar=Row2.

---

### Pitfall 3: Elapsed time label ColumnSpan conflict with pause button

**What goes wrong:** The elapsed time label currently has `Grid.ColumnSpan="2"` spanning both columns of Row 1. If the pause button is moved to `Grid.Row="1" Grid.Column="1"`, it will conflict with the spanned label, potentially hiding behind it or causing overlap.

**How to avoid:** Remove `Grid.ColumnSpan="2"` from the elapsed time label when moving it to Col 0 only. The button occupies Col 1.

---

### Pitfall 4: Banner height unknown at animation start

**What goes wrong:** `BannerGrid.Height` returns 0 or -1 before first layout pass. Using `TranslateTo(0, -BannerGrid.Height, ...)` causes no animation on first show.

**How to avoid:** Use a fixed pixel offset for initial translate (e.g., `TranslationY = -60`), not a height-relative offset. Or skip TranslationY entirely and just use FadeTo for simplicity.

---

### Pitfall 5: DataTrigger on Button border — no default reset

**What goes wrong:** DataTrigger Setter applies `BorderWidth=2` when value is True, but MAUI DataTriggers do NOT automatically reset to original value when condition becomes False unless you add a second DataTrigger for Value="False" or define defaults.

**How to avoid:** Add a second DataTrigger for `Value="False"` that sets `BorderWidth=0` and `BorderColor=Transparent`. Or set default `BorderWidth="0"` and `BorderColor="Transparent"` on the Button directly — MAUI DataTrigger will then restore to those values when condition becomes False.

Recommended: set `BorderWidth="0"` on all rhythm buttons as the baseline. DataTrigger for `Value="True"` adds `BorderWidth="2" BorderColor="White"`. When rhythm changes and the old button's IsRhythmX property goes False, MAUI restores to the declared BorderWidth="0" baseline.

---

## Code Examples

### Banner overlay — MainPage.xaml (before/after)

```xml
<!-- BEFORE: 2 rows -->
<Grid RowDefinitions="Auto,*">
    <controls:NotificationBanner Grid.Row="0" ZIndex="10" />
    <ScrollView Grid.Row="1"> ... </ScrollView>
</Grid>

<!-- AFTER: single row, overlay -->
<Grid>
    <ScrollView> ... </ScrollView>
    <controls:NotificationBanner VerticalOptions="Start" ZIndex="10" />
</Grid>
```

Note: ScrollView comes first in document order — banner declared after is rendered on top (higher in visual stack). ZIndex="10" reinforces this.

### TimerCard.xaml Row 1 change (elapsed + pause button)

```xml
<!-- Elapsed time — Col 0 only (remove ColumnSpan) -->
<Label Grid.Row="1" Grid.Column="0"
       Text="{Binding Elapsed, StringFormat='{0:mm\\:ss}'}"
       FontSize="20" FontAttributes="Bold"
       HorizontalTextAlignment="Start"
       ... />

<!-- Pause button — moved to Row 1, Col 1 (was Row 2) -->
<Button Grid.Row="1" Grid.Column="1"
        WidthRequest="36" HeightRequest="36"
        FontSize="16" Padding="0"
        BackgroundColor="Transparent"
        TextColor="#FF6F00"
        Text="{Binding IsPaused, Converter={StaticResource PauseIconConverter}}"
        Command="{Binding PauseCommand, Source={x:Reference TimerCardControl}}"
        IsVisible="{Binding ShowPauseButton, Source={x:Reference TimerCardControl}}" />

<!-- Progress bar — moved to Row 2 (was Row 3) -->
<ProgressBar Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2" ... />
```

### RhythmSelector.xaml — active button highlight (RCE example)

```xml
<Button Grid.Column="0"
        Text="RCE"
        Command="{Binding SelectRhythmCommand}"
        CommandParameter="{x:Static models:CardiacRhythm.RCE}"
        BackgroundColor="#2E7D32"
        TextColor="White" FontSize="11" FontAttributes="Bold"
        HeightRequest="36" CornerRadius="8" Padding="2,0"
        BorderWidth="0"
        BorderColor="Transparent">
    <Button.Triggers>
        <DataTrigger TargetType="Button"
                     Binding="{Binding IsRhythmRCE}"
                     Value="True">
            <Setter Property="BorderColor" Value="White" />
            <Setter Property="BorderWidth" Value="2" />
        </DataTrigger>
    </Button.Triggers>
</Button>
```

---

## State of the Art

| Old Approach | Current Approach | Impact |
|--------------|------------------|--------|
| Banner in dedicated Auto row | Banner overlaid via Grid same-row stacking | Recovers ~48dp of vertical space |
| TimerCard 4-row layout (EN PAUSA text) | TimerCard 3-row uniform layout | Uniform card heights, less visual noise |
| RITMO CARDÍACO header label | Removed — subtitle still shows "Ritmo actual: X" | ~24dp vertical space recovered, less redundancy |
| No active rhythm indicator | White border on active button | Immediate visual confirmation of selected rhythm |

---

## Open Questions

1. **Banner animation — first-render height**
   - What we know: `BannerGrid.Height` is unreliable before first layout pass
   - What's unclear: Whether CommunityToolkit.Maui's `TranslateTo` measures height internally
   - Recommendation: Use fixed -60px TranslationY offset for slide-in, not height-based. Test on device.

2. **Border visibility on dark backgrounds**
   - What we know: All rhythm buttons have colored backgrounds (green, orange, red)
   - What's unclear: Whether `BorderColor="White"` with `BorderWidth="2"` is visible enough against all button colors
   - Recommendation: White border is universally high-contrast on all these saturated backgrounds. No issue expected.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — .NET MAUI project with no unit test project in solution |
| Config file | none |
| Quick run command | Build: `dotnet build AclsTracker/AclsTracker.csproj` |
| Full suite command | Manual device/emulator testing (no automated test runner found) |

### Phase Requirements → Test Map
| ID | Behavior | Test Type | Automated Command | File Exists? |
|----|----------|-----------|-------------------|-------------|
| UI-08-A | Banner overlays content, no layout space consumed | manual | N/A | ❌ Wave 0 |
| UI-08-B | TimerCard uniform 3-row height across all 6 cards | manual | N/A | ❌ Wave 0 |
| UI-08-C | "T.Comp" appears as compressions timer name | manual | N/A | ❌ Wave 0 |
| UI-08-D | Active rhythm button shows white border | manual | N/A | ❌ Wave 0 |
| UI-08-E | "RITMO CARDÍACO" header removed | manual | N/A | ❌ Wave 0 |
| BUILD | Project compiles without errors | build | `dotnet build AclsTracker/AclsTracker.csproj` | ✅ |

### Sampling Rate
- **Per task commit:** `dotnet build AclsTracker/AclsTracker.csproj`
- **Per wave merge:** Build passes + manual visual check on Android emulator
- **Phase gate:** All 5 manual UI checks pass on Android emulator before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] No unit test project exists — all verification is manual + build check
- [ ] Manual test checklist will be defined in PLAN.md verification tasks

*(No automated test infrastructure to create — project has no test project. Build verification is the only automated check.)*

---

## Sources

### Primary (HIGH confidence)
- Source code analysis: `MainPage.xaml`, `TimerCard.xaml`, `RhythmSelector.xaml`, `NotificationBanner.xaml.cs`, `TimerViewModel.cs`, `EventRecordingViewModel.cs`, `MainViewModel.cs` — direct code reading
- Established in-project patterns: DataTrigger on Button (lines 111-117 MainPage.xaml), IsVisible binding (TimerCard), BoolToColorConverter, PauseIconConverter

### Secondary (MEDIUM confidence)
- MAUI Grid same-cell overlap: documented MAUI behavior, confirmed by in-project example at MainPage.xaml lines 33-52 (INICIAR/FINALIZAR button overlap in single Grid cell)
- CommunityToolkit.Maui FadeTo/TranslateTo: standard animation API, used in MetronomePulse (established pattern)

### Tertiary (LOW confidence)
- None — all findings are from direct code reading or confirmed MAUI primitives

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new packages, all existing MAUI primitives
- Architecture: HIGH — all patterns directly observed in existing codebase
- Pitfalls: HIGH — derived from direct code analysis (ColumnSpan, Row index shifts, DataTrigger reset behavior are well-known MAUI specifics)

**Research date:** 2026-04-05
**Valid until:** N/A — pure XAML refactor, no external dependencies
