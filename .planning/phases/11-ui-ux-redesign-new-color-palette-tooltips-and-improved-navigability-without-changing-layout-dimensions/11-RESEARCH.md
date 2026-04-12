# Phase 11: UI/UX Redesign — Research

**Researched:** 2026-04-12
**Domain:** .NET MAUI XAML theming, Shell navigation, CommunityToolkit.Maui Popup, clinical color design
**Confidence:** HIGH (codebase inspected, official docs verified)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **Color palette direction:** Clinical/professional aesthetic — clean whites, muted blues/grays, subtle accents. Hospital-equipment feel.
- **Light-mode as default**, dark mode supported
- **Semantic rhythm colors preserved** (red=shockable TV/FV, yellow=non-shockable AESP/Asistolia, green=RCE), but specific tones, shadows, and selection borders may be refined
- **Consolidate all colors into a central ResourceDictionary** with named styles (`ActionButtonPrimary`, `TimerCardBackground`, `RhythmShockable`, etc.) — eliminate hardcoded inline colors in XAML
- **Tooltip content:** Protocol + usage combined (what FCT% means, what each timer does, rhythm categories, dosing info, how buttons work)
- **Tooltip trigger:** Small ⓘ icon next to elements that have a tooltip
- **Tooltip dismiss:** Auto-dismiss 3-4 seconds + tap to close earlier
- **Tooltips always available**, even during active codes
- **Remove EventLogPanel from MainPage completely** — events continue to be recorded internally but the live feed disappears from the Principal tab
- **Events remain visible from Historial tab**
- **Bottom Shell tab bar hidden by default** to maximize screen space
- **Tab bar revealed via gesture or toggle** (swipe up, or small button)
- **3 tabs maintained:** Principal, Causas Reversibles, Historial
- **No tab renames, no tab additions/removals**

### Claude's Discretion
- Exact color values of the clinical palette (specific blue, gray, white tones)
- Technical implementation of the ResourceDictionary (App.xaml vs separate file, naming structure)
- Exact visual design of the ⓘ icon (size, position, color)
- Tooltip implementation (custom popup, overlay, ContentView)
- Technical mechanism for hiding/showing tab bar (Shell.TabBarIsVisible, custom renderer, gesture recognizer)
- Which specific elements get tooltips (prioritize most useful)
- Transition/animation when showing/hiding tab bar

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

---

## Summary

Phase 11 is a visual and navigability overhaul of the ACLS Tracker app. The codebase currently uses hardcoded hex colors scattered across 7 controls and 9 views. The `App.xaml` already merges `Colors.xaml` and `Styles.xaml` from `Resources/Styles/` — this is the correct centralization point. The existing `NotificationBanner` provides a proven auto-dismiss overlay pattern that can be adapted for tooltips. `CommunityToolkit.Maui` is already installed (v9.0.0) and actively used for `Popup` — the `ShowPopup`/`ClosePopupAsync` pattern is available for tooltip overlays.

The three major work streams are: (1) color palette centralization into `Colors.xaml` with semantic named resources, (2) tooltip system using a `TooltipOverlay` ContentView modeled on `NotificationBanner`, and (3) collapsible tab bar via `Shell.SetTabBarIsVisible(this, bool)` called from code-behind. XAML binding to `Shell.TabBarIsVisible` has known reliability issues — the code-behind static method is the recommended approach (HIGH confidence, confirmed in official docs).

**Primary recommendation:** Extend `Colors.xaml` with semantic named colors, replace all inline colors with `StaticResource`/`AppThemeBinding` references, build `TooltipOverlay` as a `ContentView` with `IDispatcherTimer` auto-dismiss mirroring `NotificationBanner`, and toggle the tab bar via `Shell.SetTabBarIsVisible()` in code-behind with a small floating toggle button.

---

## Standard Stack

### Core (already installed)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.Maui.Controls | 10.0.10 | XAML controls, Shell, ResourceDictionary | Framework itself |
| CommunityToolkit.Maui | 9.0.0 | `Popup` for tooltips, `Snackbar`, animations | Already installed, used for `PatientDataPopup` |
| CommunityToolkit.Mvvm | 8.2.2 | `[ObservableProperty]` for tab bar state | Already used throughout ViewModels |

### No New Packages Required
All needed functionality is covered by the existing stack. No new NuGet packages needed for this phase.

---

## Architecture Patterns

### Recommended File Structure Changes
```
AclsTracker/
├── Resources/Styles/
│   ├── Colors.xaml          # EXTEND with semantic named colors (clinical palette)
│   └── Styles.xaml          # EXTEND with named button/card styles
├── Controls/
│   ├── TooltipOverlay.xaml  # NEW — tooltip ContentView with auto-dismiss
│   ├── TooltipOverlay.xaml.cs
│   ├── TimerCard.xaml       # UPDATE — colors → resources, add ⓘ label
│   ├── MetronomePulse.xaml  # UPDATE — colors → resources
│   ├── RhythmSelector.xaml  # UPDATE — colors → resources, add ⓘ
│   └── NotificationBanner.xaml  # UPDATE — colors → resources
├── Views/
│   ├── MainPage.xaml        # UPDATE — remove EventLogPanel, tab bar toggle btn
│   └── MainPage.xaml.cs     # UPDATE — Shell.SetTabBarIsVisible() calls
└── AppShell.xaml            # UPDATE — Shell.TabBarIsVisible default behavior
```

### Pattern 1: Semantic Color ResourceDictionary

**What:** Extend `Colors.xaml` with semantic named colors for every visual role in the app. Replace all hardcoded inline hex values throughout all XAML files with `StaticResource` or `AppThemeBinding` references.

**When to use:** For all color assignments in XAML — never hardcode hex values directly in control markup.

```xml
<!-- Source: Colors.xaml — new semantic tokens to add -->

<!-- Clinical base palette (light) -->
<Color x:Key="SurfaceBackground">#FFFFFF</Color>
<Color x:Key="SurfaceCard">#F4F6F9</Color>
<Color x:Key="SurfaceBorder">#DDE2EA</Color>
<Color x:Key="ContentPrimary">#1A2332</Color>
<Color x:Key="ContentSecondary">#5A6478</Color>
<Color x:Key="ContentMuted">#9AA0AE</Color>

<!-- Clinical base palette (dark overrides via AppThemeBinding) -->
<Color x:Key="SurfaceBackgroundDark">#111318</Color>
<Color x:Key="SurfaceCardDark">#1C2130</Color>
<Color x:Key="SurfaceBorderDark">#2E3547</Color>

<!-- Clinical accent -->
<Color x:Key="AccentClinical">#1A73B8</Color>
<Color x:Key="AccentClinicalDark">#2D8FD4</Color>

<!-- Semantic rhythm colors (preserved meaning, refined tones) -->
<Color x:Key="RhythmShockable">#C62828</Color>        <!-- TV/FV red -->
<Color x:Key="RhythmNonShockable">#F9A825</Color>     <!-- AESP/Asistolia amber -->
<Color x:Key="RhythmNonShockableText">#212121</Color> <!-- dark text on amber -->
<Color x:Key="RhythmRCE">#2E7D32</Color>              <!-- RCE green -->

<!-- Action button semantic colors -->
<Color x:Key="ActionStartCode">#D84315</Color>   <!-- INICIAR CODIGO -->
<Color x:Key="ActionStopCode">#546E7A</Color>    <!-- FINALIZAR CODIGO -->
<Color x:Key="ActionNewCycle">#E65100</Color>    <!-- NUEVO CICLO -->
<Color x:Key="ActionDefibrillate">#B71C1C</Color> <!-- DEFIBRILAR -->
<Color x:Key="ActionDrug">#6A1B9A</Color>        <!-- ADRENALINA/AMIODARONA -->
<Color x:Key="ActionDrugSuggested">#C62828</Color> <!-- drug when suggested -->

<!-- Metronome -->
<Color x:Key="MetronomeRest">#D32F2F</Color>
<Color x:Key="MetronomeFlash">#FF6659</Color>
<Color x:Key="MetronomeToggle">#1565C0</Color>

<!-- Timer state -->
<Color x:Key="TimerOverThresholdBackground">#FFEBEE</Color>
<Color x:Key="TimerOverThresholdText">#C62828</Color>
<Color x:Key="TimerExtraInfo">#9AA0AE</Color>

<!-- Notification banner -->
<Color x:Key="BannerBackground">#FFF8E1</Color>
<Color x:Key="BannerText">#37474F</Color>

<!-- Tooltip -->
<Color x:Key="TooltipBackground">#1A2332</Color>
<Color x:Key="TooltipText">#FFFFFF</Color>
<Color x:Key="TooltipIcon">#5A9FD4</Color>

<!-- Rhythm selection border -->
<Color x:Key="RhythmSelectedBorder">#1A2332</Color>
```

### Pattern 2: TooltipOverlay ContentView (auto-dismiss)

**What:** A `ContentView` similar to `NotificationBanner` that shows above content with a darkened overlay, a compact tooltip card, and auto-dismisses after 3-4 seconds. The ⓘ icon is a `Label` with `TapGestureRecognizer` that calls `TooltipOverlay.Show(text)`.

**Why ContentView, not CommunityToolkit Popup:** The `Popup` from CommunityToolkit triggers navigation lifecycle events (`OnNavigatingFrom`, `OnDisappearing`) which disrupts active timer state. A `ContentView` overlay within the same page keeps all VM state intact — critical for mid-code tooltips.

**When to use:** For all ⓘ tooltip triggers throughout the app.

```csharp
// Source: modeled on NotificationBanner.xaml.cs pattern
public partial class TooltipOverlay : ContentView
{
    private IDispatcherTimer? _dismissTimer;

    public void Show(string tooltipText, int autoDismissSeconds = 4)
    {
        TooltipLabel.Text = tooltipText;
        IsVisible = true;

        // Animate fade-in
        this.FadeTo(1.0, 200);

        _dismissTimer?.Stop();
        _dismissTimer = Application.Current!.Dispatcher.CreateTimer();
        _dismissTimer.Interval = TimeSpan.FromSeconds(autoDismissSeconds);
        _dismissTimer.IsRepeating = false;
        _dismissTimer.Tick += (_, _) => { _dismissTimer.Stop(); Hide(); };
        _dismissTimer.Start();
    }

    public void Hide()
    {
        _dismissTimer?.Stop();
        this.FadeTo(0, 150).ContinueWith(_ => 
            MainThread.BeginInvokeOnMainThread(() => IsVisible = false));
    }
}
```

```xml
<!-- TooltipOverlay.xaml — placed as overlay in MainPage Grid (ZIndex=20) -->
<ContentView IsVisible="False" Opacity="0">
    <Grid>
        <!-- Semi-transparent backdrop — tap to dismiss -->
        <BoxView BackgroundColor="#60000000">
            <BoxView.GestureRecognizers>
                <TapGestureRecognizer Tapped="OnBackdropTapped" />
            </BoxView.GestureRecognizers>
        </BoxView>
        <!-- Tooltip card -->
        <Frame BackgroundColor="{StaticResource TooltipBackground}"
               CornerRadius="10"
               HorizontalOptions="Center"
               VerticalOptions="Center"
               Margin="20"
               MaximumWidthRequest="280">
            <Label x:Name="TooltipLabel"
                   TextColor="{StaticResource TooltipText}"
                   FontSize="14"
                   LineBreakMode="WordWrap" />
        </Frame>
    </Grid>
</ContentView>
```

**ⓘ icon pattern** — place next to any element that needs a tooltip:
```xml
<Label Text="ⓘ"
       FontSize="13"
       TextColor="{StaticResource TooltipIcon}"
       VerticalOptions="Center">
    <Label.GestureRecognizers>
        <TapGestureRecognizer Tapped="OnFCTTooltipTapped" />
    </Label.GestureRecognizers>
</Label>
```

### Pattern 3: Collapsible Tab Bar via Shell.SetTabBarIsVisible

**What:** Tab bar is hidden by default. A small floating toggle button (hamburger / chevron-up) in the page reveals it. `Shell.SetTabBarIsVisible(this, bool)` is called from code-behind of MainPage, not via XAML binding.

**Why code-behind, not XAML binding:** XAML binding to `Shell.TabBarIsVisible` has known reliability issues in MAUI, including broken behavior with 5+ tabs and platform inconsistencies. The static method `Shell.SetTabBarIsVisible(page, bool)` is the documented, reliable approach (confirmed in official docs and Q&A).

**Apple HIG consideration:** Apple recommends tab bars always be visible. For iOS, auto-hiding may conflict with HIG. Consider making the default behavior "visible, with option to hide" on iOS, or ensuring the toggle is very discoverable.

```csharp
// Source: official MAUI docs + Q&A verified pattern
// MainPage.xaml.cs
protected override void OnAppearing()
{
    base.OnAppearing();
    Shell.SetTabBarIsVisible(this, false); // hidden by default
}

private void OnToggleTabBarTapped(object sender, EventArgs e)
{
    bool currentlyVisible = Shell.Current?.CurrentPage != null &&
        (bool)Shell.Current.CurrentPage.GetValue(Shell.TabBarIsVisibleProperty);
    Shell.SetTabBarIsVisible(this, !currentlyVisible);
}
```

```xml
<!-- Floating tab bar toggle button — placed in MainPage Grid, bottom-right -->
<Button x:Name="TabBarToggle"
        Text="≡"
        WidthRequest="40" HeightRequest="40"
        CornerRadius="20"
        BackgroundColor="{AppThemeBinding Light={StaticResource AccentClinical}, Dark={StaticResource AccentClinicalDark}}"
        TextColor="White"
        FontSize="18"
        VerticalOptions="End" HorizontalOptions="End"
        Margin="0,0,12,12"
        ZIndex="5"
        Clicked="OnToggleTabBarTapped" />
```

### Pattern 4: Remove EventLogPanel from MainPage

**What:** Delete the `<controls:EventLogPanel ... />` line from MainPage XAML (Block 6). The `EventLogPanel` control itself remains — it is moved to `HistorialPage` or stays available for future use. The underlying event recording service continues unmodified.

**Impact:** The `VerticalStackLayout` in `MainPage` shrinks by the full height of the event log panel. No layout dimension changes to the remaining 5 blocks.

### Anti-Patterns to Avoid

- **XAML binding to Shell.TabBarIsVisible property:** Known to be unreliable — use `Shell.SetTabBarIsVisible(this, bool)` in code-behind instead.
- **Using `toolkit:Popup` for tooltips:** Triggers page lifecycle events (`OnNavigatingFrom`, `OnDisappearing`) that would disrupt active timers and the ViewModel state during a code. Use a ContentView overlay inside the page's Grid instead.
- **Hardcoded colors in DataTrigger setters:** Even inside `<Setter>` inside `<DataTrigger>`, replace hex values with `{StaticResource ...}` references. This ensures the palette update propagates everywhere.
- **Duplicate color definitions:** Don't define clinical colors in `App.xaml` resource dictionary directly — keep them in `Colors.xaml` which is already merged by `App.xaml`.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Auto-dismiss timer for tooltip | Custom countdown logic | `IDispatcherTimer` (already used in `NotificationBanner`) | Thread-safe, cross-platform, already proven in codebase |
| Animated overlay show/hide | Manual opacity tweening | `this.FadeTo()` MAUI animation API | Idiomatic, handles cancellation correctly |
| Tab bar programmatic toggle | Custom renderer / platform code | `Shell.SetTabBarIsVisible(this, bool)` | Official API, no platform-specific code needed |
| Centralized app colors | Per-file color constants | `Colors.xaml` ResourceDictionary | Already the architecture in the project |

**Key insight:** The NotificationBanner already implements the entire auto-dismiss overlay pattern needed for tooltips. TooltipOverlay is a simplification of that existing control.

---

## Common Pitfalls

### Pitfall 1: Binding to Shell.TabBarIsVisible in XAML fails silently
**What goes wrong:** Setting `Shell.TabBarIsVisible="{Binding IsTabBarVisible}"` in XAML appears to work on first render but doesn't update reactively on all platforms.
**Why it happens:** `Shell.TabBarIsVisible` is an attached property and its binding infrastructure behaves differently than standard `BindableProperty` bindings in some MAUI versions.
**How to avoid:** Always toggle via `Shell.SetTabBarIsVisible(this, bool)` in code-behind or a command handler. Track the toggle state in code-behind or a simple bool field.
**Warning signs:** Tab bar visibility stuck after first binding update, inconsistent behavior between Android/iOS.

### Pitfall 2: CommunityToolkit Popup triggers page lifecycle on MainPage
**What goes wrong:** Using `this.ShowPopupAsync(new TooltipPopup())` inside MainPage fires `OnNavigatingFrom` + `OnDisappearing` on MainPage. Any code that uses these lifecycle events (timer pause, VM cleanup) would execute incorrectly.
**Why it happens:** CommunityToolkit Popup is presented modally — it's a full page navigation under the hood.
**How to avoid:** Implement tooltips as a `ContentView` within the page's root `Grid`, with `ZIndex` above content. No navigation involved.
**Warning signs:** Metronome pausing when tooltip opens.

### Pitfall 3: DataTrigger Setter colors not updated by ResourceDictionary
**What goes wrong:** Replacing a hardcoded color in a `BackgroundColor` attribute with `{StaticResource X}` works. But if the same color appears inside a `<DataTrigger><Setter Value="#XXXXXX" />`, it may not pick up the StaticResource reference in older MAUI versions.
**Why it happens:** `Setter.Value` in triggers doesn't always support markup extension syntax in all MAUI versions.
**How to avoid:** Test DataTrigger setters explicitly. If `{StaticResource}` doesn't work in a Setter, consider using a `Converter` or binding instead.
**Warning signs:** Color not changing inside DataTrigger state despite ResourceDictionary update.

### Pitfall 4: MetronomePulse color in C# code-behind, not XAML
**What goes wrong:** The metronome `RestColor` and `FlashColor` are hardcoded as `Color.FromArgb()` constants in `MetronomePulse.xaml.cs` — they won't be updated by changing `Colors.xaml`.
**Why it happens:** The animation requires programmatic color interpolation in C#, not XAML binding.
**How to avoid:** Update `MetronomePulse.xaml.cs` code constants to match the new palette values directly. Use color values that match the semantic resources.
**Warning signs:** Metronome circle staying on old red after palette update.

### Pitfall 5: iOS tab bar auto-hide and Apple HIG
**What goes wrong:** App Store reviewers may flag tab bar being hidden by default as violating HIG.
**Why it happens:** Apple guidelines state tab bars should always be visible for navigation predictability.
**How to avoid:** Consider making the tab bar visible by default on iOS, with the hide feature still available as a user gesture. Or ensure the toggle button is very obvious.
**Warning signs:** App Store rejection for "tab bar hidden, navigation unclear."

---

## Code Examples

### Full replacement pattern: inline color → StaticResource

```xml
<!-- BEFORE (TimerCard.xaml) -->
<Frame BackgroundColor="{AppThemeBinding Light=#F5F5F5, Dark=#1E1E1E}"
       BorderColor="{AppThemeBinding Light=#E0E0E0, Dark=#333333}">

<!-- AFTER -->
<Frame BackgroundColor="{AppThemeBinding Light={StaticResource SurfaceCard}, Dark={StaticResource SurfaceCardDark}}"
       BorderColor="{AppThemeBinding Light={StaticResource SurfaceBorder}, Dark={StaticResource SurfaceBorderDark}}">
```

### DataTrigger with resource color

```xml
<!-- BEFORE -->
<DataTrigger TargetType="Frame" Binding="{Binding IsOverThreshold}" Value="True">
    <Setter Property="BackgroundColor" Value="#FFEBEE" />
</DataTrigger>

<!-- AFTER -->
<DataTrigger TargetType="Frame" Binding="{Binding IsOverThreshold}" Value="True">
    <Setter Property="BackgroundColor" Value="{StaticResource TimerOverThresholdBackground}" />
</DataTrigger>
```

### Rhythm button active border — cleaner selection indicator

```xml
<!-- Replace "Black 3px border" with a clinical dark navy + slight shadow effect -->
<DataTrigger TargetType="Button" Binding="{Binding IsRhythmTV}" Value="True">
    <Setter Property="BorderColor" Value="{StaticResource RhythmSelectedBorder}" />
    <Setter Property="BorderWidth" Value="2.5" />
</DataTrigger>
```

### Shell.SetTabBarIsVisible in OnAppearing

```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs
protected override void OnAppearing()
{
    base.OnAppearing();
    Shell.SetTabBarIsVisible(this, false);
}
```

### TooltipOverlay usage from page code-behind

```csharp
// MainPage.xaml.cs
private void OnFCTInfoTapped(object sender, EventArgs e)
{
    TooltipOverlay.Show(
        "FCT% (Fracción de Compresión Torácica): Porcentaje del tiempo del código con compresiones activas. " +
        "La AHA recomienda mantener FCT ≥ 60%.",
        autoDismissSeconds: 4
    );
}
```

---

## Tooltip Content Map (Priority Elements)

Based on the codebase, these are the highest-value tooltip targets:

| Element | Tooltip Content | Priority |
|---------|----------------|----------|
| FCT% (ExtraInfo on Compresiones timer) | "FCT% = tiempo en compresiones / tiempo total código. AHA recomienda ≥60%." | HIGH |
| Ciclo # (ExtraInfo on Ciclo RCP timer) | "Cada ciclo = 2 min de RCP. Se resetea al presionar NUEVO CICLO." | HIGH |
| Dose count (ExtraInfo on Adrenalina/Amiodarona) | "Número de dosis administradas. Adrenalina: cada 3-5 min. Amiodarona: máx 3 dosis." | HIGH |
| DEFIBRILAR button | "Solo para ritmos desfibrilables (TV/FV). 200J bifásico inicial. Sin pulso — CPR inmediata post-descarga." | HIGH |
| RCE rhythm button | "Retorno de Circulación Espontánea — pulso palpable. Cambia el protocolo al manejo post-RCE." | MEDIUM |
| AESP/Asistolia buttons | "No desfibrilable. Buscar H's y T's. Adrenalina cada 3-5 min." | MEDIUM |
| Metronome BPM display | "Frecuencia de compresiones. AHA: 100-120 por minuto." | MEDIUM |
| T. Pulso timer | "Tiempo sin pulso tras desfibrilación o cambio de ritmo. Rojo = >10 segundos." | LOW |

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Hardcoded hex inline in every XAML | StaticResource/AppThemeBinding from Colors.xaml | This phase | Single-source-of-truth for palette |
| No tooltips | TooltipOverlay ContentView with ⓘ icons | This phase | Protocol guidance in-app |
| Tab bar always visible | Hidden by default, toggle to reveal | This phase | More vertical space on Principal |
| EventLogPanel in MainPage | Moved to Historial tab | This phase | Simpler Principal view |

**Deprecated in this phase:**
- All inline `BackgroundColor="#XXXXXX"` in XAML controls/views
- `Color.FromArgb("#D32F2F")` constants in MetronomePulse.xaml.cs (replace with matching resource values)

---

## Open Questions

1. **iOS App Store: Tab bar auto-hide**
   - What we know: Apple HIG recommends tab bars stay visible
   - What's unclear: Whether the specific implementation (hidden by default, toggle button) would cause App Store rejection
   - Recommendation: Implement with toggle button prominently visible; if App Store is a concern, default to visible on iOS only using `#if IOS` or platform-specific code

2. **DataTrigger Setter with StaticResource in MAUI 10**
   - What we know: In some MAUI versions, `<Setter Value="{StaticResource X}">` inside DataTrigger doesn't resolve correctly
   - What's unclear: Whether MAUI 10.0.10 (current version) has fixed this
   - Recommendation: Test immediately when implementing — fallback is to use a style with a binding and converter instead of DataTrigger

3. **CommunityToolkit.Maui Popup lifecycle in MAUI 10**
   - What we know: Popup fires page lifecycle events. There's an open bug (dotnet/maui#34073) about OnNavigatingFrom not working correctly
   - What's unclear: Whether this bug affects our specific auto-dismiss timer case
   - Recommendation: Proceed with ContentView overlay approach (not Popup) to avoid lifecycle issues entirely

---

## Validation Architecture

No formal test infrastructure exists for UI/styling (confirmed: no test project in codebase). This phase is UI-only; validation is visual/manual.

### Phase Requirements → Test Map
| Behavior | Test Type | Validation Command |
|----------|-----------|-------------------|
| Colors render from ResourceDictionary (no inline hex in production builds) | Manual — inspect XAML | grep for `#[0-9A-F]` in XAML after implementation |
| Tooltips appear and auto-dismiss | Manual — tap each ⓘ icon | Device/emulator test |
| Tooltips dismissable by tap | Manual — tap backdrop | Device/emulator test |
| Tab bar hides on MainPage appear | Manual — launch app | Emulator |
| Tab bar toggle reveals/hides tabs | Manual — tap toggle button | Emulator |
| EventLogPanel absent from MainPage | Manual — scroll MainPage | Emulator + code review |
| Timers continue during tooltip display | Manual — start code, show tooltip | Emulator |
| Dark mode: all semantic colors render correctly | Manual — switch OS theme | Emulator (Android) |

### Wave 0 Gaps
- No automated test infrastructure needed — this is pure UI/styling work
- Manual verification checklist sufficient

---

## Sources

### Primary (HIGH confidence)
- [.NET MAUI Shell Tabs — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs?view=net-maui-10.0) — TabBarIsVisible, Shell.SetTabBarIsVisible, tab appearance properties
- [CommunityToolkit.Maui Popup — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup) — ShowPopupAsync, ClosePopupAsync, auto-dismiss pattern, lifecycle behavior
- Codebase inspection: `App.xaml`, `Colors.xaml`, `Styles.xaml`, `NotificationBanner.xaml.cs`, `PatientDataPopup.xaml`, `MetronomePulse.xaml.cs`, `TimerCard.xaml`, `RhythmSelector.xaml`, `MainPage.xaml`, `AppShell.xaml`, `MauiProgram.cs`, `AclsTracker.csproj`

### Secondary (MEDIUM confidence)
- [How to make MAUI Shell TabBar conditionally visible — Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/1350040/how-to-make-maui-shell-tabbar-conditionally-visibl) — code-behind toggle pattern, Apple HIG warning
- ui-ux-pro-max skill: `--design-system` healthcare/medical query — clinical palette guidance (#0891B2, #164E63, #ECFEFF family)
- ui-ux-pro-max skill: color domain search "clinical professional hospital" — B2B/professional palettes

### Tertiary (LOW confidence)
- [iOS TabBarIsVisible known issues — GitHub dotnet/maui #8017](https://github.com/dotnet/maui/issues/8017) — platform-specific bugs (may be fixed in MAUI 10)
- [iOS TabBar overlaps content after SetTabBarIsVisible — GitHub dotnet/maui #10591](https://github.com/dotnet/maui/issues/10591) — layout issue after programmatic show

---

## Metadata

**Confidence breakdown:**
- Color palette centralization: HIGH — existing `Colors.xaml` infrastructure is in place, pattern is clear
- TooltipOverlay implementation: HIGH — `NotificationBanner` provides exact pattern to adapt
- Tab bar collapsible: MEDIUM-HIGH — `Shell.SetTabBarIsVisible()` is documented and verified; iOS HIG risk is a real concern but manageable
- DataTrigger + StaticResource: MEDIUM — known potential issue in older MAUI; needs early verification on MAUI 10.0.10

**Research date:** 2026-04-12
**Valid until:** 2026-05-12 (stable APIs; check MAUI release notes for any 10.x changes)
