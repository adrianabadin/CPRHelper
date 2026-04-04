# Phase 7: Modificar Frontend — Botones Adrenalina y Amiodarona en Primera Vista - Research

**Researched:** 2026-04-04
**Domain:** .NET MAUI XAML layout optimization — vertical space compression
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- INICIAR/FINALIZAR CODIGO se mantiene arriba (posición actual)
- El orden de bloques no cambia: INICIAR → Metrónomo → Ritmo → Timers → Acciones → EventLog
- El espacio se gana reduciendo alturas y comprimiendo, no reordenando
- Shell title bar y AuthAvatarControl se mantienen como están
- Los 5 botones de ritmo deben entrar en UNA sola fila horizontal
- Reducir font a 11-12px y HeightRequest a ~36px en botones de ritmo
- Reemplazar FlexLayout wrap por Grid o HorizontalStackLayout de fila única en RhythmSelector
- Fila 1 de acciones: NUEVO CICLO + DEFIBRILAR lado a lado (50/50 igual ancho)
- Fila 2 de acciones: ADRENALINA + AMIODARONA lado a lado (como están)
- Eliminar DEFIBRILAR como botón de ancho completo — pasa a compartir fila con NUEVO CICLO
- HeightRequest de los 4 botones de acción: 40px (reducido de 44px)
- Colores sin cambios: NUEVO CICLO naranja (#FF9800), DEFIBRILAR rojo (#C62828), drogas violeta (#7B1FA2)
- Metrónomo: círculo animado de 50px → 45px (Grid WidthRequest/HeightRequest)
- TimerCards: padding interno de 8px → 4px, font elapsed de 24px → 20px
- Botón INICIAR CODIGO: mantener 48px (es el botón más importante)
- DataTrigger highlights en ADRENALINA y AMIODARONA deben mantenerse funcionales
- BoolToOpacityConverter para AMIODARONA debe mantenerse funcional

### Claude's Discretion
- Valores exactos de Spacing y Padding del VerticalStackLayout (actualmente Spacing=8, Padding=12)
- RowSpacing/ColumnSpacing del grid de timers (actualmente 6px)
- Ajustes finos de font sizes en TimerCard labels

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope
</user_constraints>

---

## Summary

This phase is a pure layout compression exercise on four existing XAML files. No new logic, ViewModels, commands, converters, or services are introduced. The problem is well-defined: the FlexLayout in RhythmSelector wraps its 5 buttons to 2 rows on standard screens, and DEFIBRILAR occupies a full-width row that could be shared — together these two layout inefficiencies push ADRENALINA and AMIODARONA below the fold.

The vertical budget analysis confirms the issue. A standard Android screen gives approximately 760-800dp of usable height below the Shell title bar. The current layout consumes approximately 780-800dp before reaching ADRENALINA/AMIODARONA, with the FlexLayout wrapping being the single largest contributor (the wrap alone adds ~50-55dp when 5 buttons spill to a second row). Collapsing that to a single row and merging NUEVO CICLO + DEFIBRILAR into one row recovers ~90-100dp — enough to bring the drug buttons into the first viewport.

All changes are mechanical XAML edits. The planner should treat each file as an independent task, ordered by impact: RhythmSelector first (largest gain), then MainPage action grid, then MetronomePulse circle, then TimerCard sizing.

**Primary recommendation:** Edit four XAML files in sequence. No code-behind changes are required for any of the locked decisions.

---

## Vertical Space Audit

Current layout stack height estimate (dp, standard Android ~420dpi device, ~800dp usable):

| Block | Current Height | After Change | Savings |
|-------|---------------|--------------|---------|
| INICIAR button | 48 | 48 | 0 |
| VSL spacing ×N (Spacing=8) | 8×5 = 40 | 8×5 = 40 | 0 |
| MetronomePulse (Grid 50 + Padding 8,4) | ~58 | ~53 | ~5 |
| RhythmSelector header labels | ~44 | ~44 | 0 |
| RhythmSelector FlexLayout (2 rows) | ~104 | ~44 | ~60 |
| Timer Grid (3 rows, Frame Padding=8, elapsed FontSize=24) | ~252 | ~224 | ~28 |
| NUEVO CICLO button | 44 | — | — |
| DEFIBRILAR button | 44 | 40 (merged) | — |
| NUEVO CICLO + DEFIBRILAR merged row | — | 40 | ~48 total savings vs 2 rows |
| ADRENALINA + AMIODARONA row | 44 | 40 | 4 |
| VSL Padding top+bottom (12+12) | 24 | 18-20 | ~4-6 |
| **Total estimated** | **~762** | **~651-665** | **~97-111** |

The rhythm selector row collapse (~60dp) and the action button row merge (~48dp) together provide the critical ~108dp needed to clear ADRENALINA/AMIODARONA into the viewport.

---

## Standard Stack

### Core
| Component | Current File | Change Required |
|-----------|-------------|-----------------|
| VerticalStackLayout | MainPage.xaml | Reduce Spacing and Padding |
| FlexLayout (rhythm buttons) | RhythmSelector.xaml | Replace with HorizontalStackLayout or Grid |
| Grid (action buttons) | MainPage.xaml lines 88-145 | Restructure to 2-row 2-column Grid |
| Frame + Grid (TimerCard) | TimerCard.xaml | Reduce Frame Padding, Label FontSize |
| Grid (MetronomePulse circle) | MetronomePulse.xaml | Reduce WidthRequest/HeightRequest |

No new NuGet packages needed. All layouts use standard .NET MAUI controls already in use.

---

## Architecture Patterns

### Files to Edit (in order of impact)

```
AclsTracker/Controls/RhythmSelector.xaml      # Largest gain — collapse FlexLayout to single row
AclsTracker/Views/MainPage.xaml               # Merge NUEVO CICLO + DEFIBRILAR, reduce padding
AclsTracker/Controls/TimerCard.xaml           # Reduce Frame Padding, elapsed FontSize
AclsTracker/Controls/MetronomePulse.xaml      # Reduce circle Grid size
```

No `.xaml.cs` files require changes. All locked decisions are pure XAML property edits.

### Pattern 1: Replace FlexLayout with HorizontalStackLayout (RhythmSelector)

**What:** The current FlexLayout with `Wrap="Wrap"` allows buttons to spill to a second row. A
`HorizontalStackLayout` with no wrap enforces a single row. To fit 5 buttons, font must shrink
to 11-12px and HeightRequest to 36px. The `Margin="4"` on each button also contributes to width;
reducing to `Margin="2"` or using `Spacing` on the parent is cleaner.

**When to use:** Any time a button row must stay single-line on small screens.

**Example (after):**
```xml
<!-- Replace FlexLayout block in RhythmSelector.xaml -->
<HorizontalStackLayout Spacing="4" HorizontalOptions="Center">
    <Button Text="RCE"
            Command="{Binding SelectRhythmCommand}"
            CommandParameter="{x:Static models:CardiacRhythm.RCE}"
            BackgroundColor="#2E7D32"
            TextColor="White" FontSize="12" FontAttributes="Bold"
            HeightRequest="36" CornerRadius="8" Padding="4,0" />
    <!-- repeat for AESP, ASISTOLIA, TV, FV -->
</HorizontalStackLayout>
```

**Note on ASISTOLIA:** This is the longest label (9 chars). At FontSize="12" and Padding="4,0",
it needs approximately 80dp minimum. With 5 buttons and 4 gaps of 4dp = 4×4=16dp, total =
5×80 + 16 = 416dp. Standard Android screen width ~360dp. This is tight — ASISTOLIA may need
`FontSize="11"` and `Padding="2,0"`. A Grid with equal column widths is safer for guaranteed
single-row layout:

```xml
<!-- Grid approach guarantees equal sizing on all screen widths -->
<Grid ColumnDefinitions="*, *, *, *, *" ColumnSpacing="4">
    <Button Grid.Column="0" Text="RCE"     ... FontSize="11" HeightRequest="36" Padding="2,0" />
    <Button Grid.Column="1" Text="AESP"    ... FontSize="11" HeightRequest="36" Padding="2,0" />
    <Button Grid.Column="2" Text="ASISTOLIA" ... FontSize="11" HeightRequest="36" Padding="2,0" />
    <Button Grid.Column="3" Text="TV"      ... FontSize="11" HeightRequest="36" Padding="2,0" />
    <Button Grid.Column="4" Text="FV"      ... FontSize="11" HeightRequest="36" Padding="2,0" />
</Grid>
```

The Grid with `*` columns is the recommended approach — each button gets exactly 1/5 of
available width regardless of label length, and ASISTOLIA will never overflow.

### Pattern 2: Merge NUEVO CICLO + DEFIBRILAR into a 2-column Grid row (MainPage)

**What:** Replace the two consecutive full-width Button elements (NUEVO CICLO and DEFIBRILAR)
and the existing ADRENALINA+AMIODARONA Grid with a single outer Grid that has 2 rows and 2 columns.

**When to use:** When 2 buttons need to share a row with equal width.

**Example (after):**
```xml
<!-- BLOCK 5: Botones de accion — 2 rows x 2 columns -->
<Grid ColumnDefinitions="*, *" RowDefinitions="Auto, Auto"
      ColumnSpacing="8" RowSpacing="6">
    <!-- Row 0: NUEVO CICLO + DEFIBRILAR -->
    <Button Grid.Row="0" Grid.Column="0"
            Text="NUEVO CICLO"
            Command="{Binding NewCycleCommand}"
            BackgroundColor="#FF9800"
            TextColor="White" FontSize="14" FontAttributes="Bold"
            HeightRequest="40" CornerRadius="8" />
    <Button Grid.Row="0" Grid.Column="1"
            x:Name="DefibrilarButton"
            Text="DEFIBRILAR"
            Command="{Binding DefibrilarCommand}"
            BackgroundColor="#C62828"
            TextColor="White" FontSize="14" FontAttributes="Bold"
            HeightRequest="40" CornerRadius="8" />
    <!-- Row 1: ADRENALINA + AMIODARONA (DataTriggers preserved) -->
    <Button Grid.Row="1" Grid.Column="0"
            Text="ADRENALINA"
            Command="{Binding AdrenalinaCommand}"
            BackgroundColor="#7B1FA2"
            TextColor="White" FontSize="14" FontAttributes="Bold"
            HeightRequest="40" CornerRadius="8">
        <Button.Triggers>
            <DataTrigger TargetType="Button"
                         Binding="{Binding IsAdrenalinaSuggested}"
                         Value="True">
                <Setter Property="BackgroundColor" Value="#D32F2F" />
            </DataTrigger>
        </Button.Triggers>
    </Button>
    <Button Grid.Row="1" Grid.Column="1"
            Text="AMIODARONA"
            Command="{Binding AmiodaronaCommand}"
            IsEnabled="{Binding IsAmiodaronaEnabled}"
            Opacity="{Binding IsAmiodaronaEnabled, Converter={StaticResource BoolToOpacityConverter}}"
            BackgroundColor="#7B1FA2"
            TextColor="White" FontSize="14" FontAttributes="Bold"
            HeightRequest="40" CornerRadius="8">
        <Button.Triggers>
            <DataTrigger TargetType="Button"
                         Binding="{Binding IsAmiodaronaSuggested}"
                         Value="True">
                <Setter Property="BackgroundColor" Value="#D32F2F" />
            </DataTrigger>
        </Button.Triggers>
    </Button>
</Grid>
```

This replaces lines 88-145 in MainPage.xaml entirely. The three separate elements
(NUEVO CICLO Button, DEFIBRILAR Button, ADRENALINA+AMIODARONA Grid) become one unified Grid.
The VSL sees one child instead of three, removing 2 inter-block Spacing=8 gaps (16dp saved).

### Pattern 3: Reduce MetronomePulse circle (MetronomePulse.xaml)

**What:** The outer Grid container uses `WidthRequest="50" HeightRequest="50"`. Change to 45.
The inner Ellipse uses `WidthRequest="44" HeightRequest="44"`. Change to 40. No animation
references point to the Grid size — `PulseCircle` is the Ellipse name used in code-behind
for the scale animation.

**Verify in MetronomePulse.xaml.cs:** Confirm no hardcoded size references to 50 or 44 in
the animation code before changing.

**Example (after):**
```xml
<Grid WidthRequest="45" HeightRequest="45">
    <Ellipse x:Name="PulseCircle"
             WidthRequest="40" HeightRequest="40"
             ... />
</Grid>
```

### Pattern 4: Reduce TimerCard padding and elapsed font (TimerCard.xaml)

**What:** Frame `Padding="8"` → `Padding="4"`. Elapsed time Label `FontSize="24"` → `FontSize="20"`.
These are the two properties that contribute most to each TimerCard's height. With 3 rows of
2 cards each, reducing each card height by ~12dp saves ~36dp across the timer grid.

**Example (after):**
```xml
<Frame Padding="4"
       ...>
    ...
    <!-- Digital elapsed display -->
    <Label Grid.Row="1" ...
           FontSize="20"
           ... />
```

### Pattern 5: VerticalStackLayout Spacing and Padding (MainPage.xaml — Claude's Discretion)

**Recommended values:**
- `Padding="10"` (down from 12) — saves 4dp (2dp each side)
- `Spacing="6"` (down from 8) — saves 2dp per gap; with 5 inter-block gaps after restructuring
  (action buttons collapse from 3 items to 1 Grid), that's 5×2=10dp saved

The action button block restructuring from 3 VSL children to 1 Grid also removes 2 spacing
gaps entirely (2×8=16dp additional saving even at the original Spacing=8).

### Anti-Patterns to Avoid

- **Removing the Margin="2" on TimerCard Frame:** The Frame already has `Margin="2"` that provides
  visual separation. Removing it causes cards to touch each other visually. Keep it.
- **Setting MinimumWidthRequest on rhythm buttons inside a Grid:** MinimumWidthRequest conflicts
  with Grid column `*` sizing. Remove MinimumWidthRequest when switching from FlexLayout to Grid.
- **Changing RowSpacing/ColumnSpacing to 0 on the timer grid:** Zero spacing makes the 6 timer
  cards visually merge into a single block. Recommended: keep at 4-6px.
- **Removing BoolToOpacityConverter from AMIODARONA:** The `Opacity` binding is the visual
  disabled indicator for AMIODARONA (enabled only after first adrenalina dose). Must be preserved.
- **Forgetting to remove `x:Name="DefibrilarButton"` risk:** Check MetronomePulse.xaml.cs and
  MainPage.xaml.cs for references to `DefibrilarButton` by name. If used in code-behind, the
  name must survive the restructure (move `x:Name` to the new Grid cell button).

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead |
|---------|-------------|-------------|
| Equal-width single row of 5 buttons | Custom measure/layout | Grid with `*` columns — built-in equal distribution |
| Responsive font size per screen width | OnIdiom/OnPlatform breakpoints | Fixed 11-12px — small enough for all standard screens |
| Animated circle size change | ScaleAnimation on resize | Static property edit to HeightRequest/WidthRequest |

---

## Common Pitfalls

### Pitfall 1: ASISTOLIA text clipping at small font
**What goes wrong:** "ASISTOLIA" (9 characters) clips or truncates at FontSize="12" with
insufficient Padding.
**Why it happens:** Button default Padding in MAUI is platform-dependent. On Android, default
button padding adds ~8dp horizontal, which inflates the minimum render width beyond the
available column width.
**How to avoid:** Set explicit `Padding="2,0"` on all 5 rhythm buttons. Use Grid `*` columns
so each gets equal width (360dp screen / 5 columns - 4×4dp spacing = ~68dp per button).
At 68dp width, "ASISTOLIA" at FontSize="11" renders without clipping.
**Warning signs:** Text shows "ASISTOL..." on emulator — reduce Padding first, then FontSize.

### Pitfall 2: DataTrigger bindings lost during Grid restructure
**What goes wrong:** ADRENALINA/AMIODARONA DataTriggers disappear because developer copies
the Button markup without the `<Button.Triggers>` block.
**Why it happens:** Visual Studio/editor paste sometimes loses nested elements.
**How to avoid:** Use the code examples above as complete Button definitions. Verify triggers
are present in the final XAML before saving.
**Warning signs:** Button stays purple even when drug suggestion is active.

### Pitfall 3: MinimumWidthRequest conflicts with Grid column *
**What goes wrong:** `MinimumWidthRequest="80"` from the original FlexLayout buttons causes
layout warnings or unexpected sizing inside a Grid.
**Why it happens:** Grid column `*` sizing ignores MinimumWidthRequest and tries to give equal
space; the constraint can cause layout overflow exceptions on narrow screens.
**How to avoid:** Remove `MinimumWidthRequest` from all rhythm buttons when placing them in a Grid.
**Warning signs:** Layout exception in debug output, or buttons overflow container.

### Pitfall 4: Code-behind animation references to circle size
**What goes wrong:** MetronomePulse.xaml.cs uses a hardcoded size (50 or 44) for the pulse
animation scale calculation.
**Why it happens:** Animation code sometimes anchors to absolute pixel values.
**How to avoid:** Read MetronomePulse.xaml.cs before editing. The `PulseCircle` Ellipse name
is used for scale animations — scale operations are multiplicative (ScaleTo(1.1)) not absolute,
so changing WidthRequest/HeightRequest does not break them.
**Warning signs:** Circle animation appears clipped or misaligned after size change.

### Pitfall 5: VSL child count change breaks spacing calculation
**What goes wrong:** The new unified Grid for action buttons reduces VSL children from 3 to 1,
but developer forgets this saves 2 spacing gaps automatically.
**Why it happens:** Developer adjusts Spacing conservatively without accounting for structural change.
**How to avoid:** After restructuring, count VSL children: INICIAR Grid (1) + MetronomePulse (1) +
RhythmSelector (1) + Timer Grid (1) + Action Grid (1) + EventLog (1) = 6 children, 5 gaps.
At Spacing=6: 5×6=30dp for spacing. Original was 8 children, 7 gaps at Spacing=8 = 56dp.
The restructure alone saves 26dp in spacing even without changing the Spacing value.

---

## Code Examples

### Complete RhythmSelector.xaml after change
```xml
<!-- Source: direct edit of AclsTracker/Controls/RhythmSelector.xaml -->
<VerticalStackLayout Spacing="6">
    <Label Text="RITMO CARDÍACO"
           FontSize="12" FontAttributes="Bold"
           TextColor="#999999"
           HorizontalTextAlignment="Center"
           CharacterSpacing="1" />
    <Label Text="{Binding CurrentRhythmDisplay, StringFormat='Ritmo actual: {0}'}"
           FontSize="13" FontAttributes="Bold"
           HorizontalTextAlignment="Center"
           TextColor="#333333" />
    <Grid ColumnDefinitions="*, *, *, *, *" ColumnSpacing="4">
        <Button Grid.Column="0" Text="RCE"
                Command="{Binding SelectRhythmCommand}"
                CommandParameter="{x:Static models:CardiacRhythm.RCE}"
                BackgroundColor="#2E7D32"
                TextColor="White" FontSize="11" FontAttributes="Bold"
                HeightRequest="36" CornerRadius="8" Padding="2,0" />
        <Button Grid.Column="1" Text="AESP"
                Command="{Binding SelectRhythmCommand}"
                CommandParameter="{x:Static models:CardiacRhythm.AESP}"
                BackgroundColor="#E65100"
                TextColor="White" FontSize="11" FontAttributes="Bold"
                HeightRequest="36" CornerRadius="8" Padding="2,0" />
        <Button Grid.Column="2" Text="ASISTOLIA"
                Command="{Binding SelectRhythmCommand}"
                CommandParameter="{x:Static models:CardiacRhythm.Asistolia}"
                BackgroundColor="#E65100"
                TextColor="White" FontSize="11" FontAttributes="Bold"
                HeightRequest="36" CornerRadius="8" Padding="2,0" />
        <Button Grid.Column="3" Text="TV"
                Command="{Binding SelectRhythmCommand}"
                CommandParameter="{x:Static models:CardiacRhythm.TV}"
                BackgroundColor="#D32F2F"
                TextColor="White" FontSize="11" FontAttributes="Bold"
                HeightRequest="36" CornerRadius="8" Padding="2,0" />
        <Button Grid.Column="4" Text="FV"
                Command="{Binding SelectRhythmCommand}"
                CommandParameter="{x:Static models:CardiacRhythm.FV}"
                BackgroundColor="#D32F2F"
                TextColor="White" FontSize="11" FontAttributes="Bold"
                HeightRequest="36" CornerRadius="8" Padding="2,0" />
    </Grid>
</VerticalStackLayout>
```

### TimerCard Frame Padding and elapsed font reduction
```xml
<!-- Source: AclsTracker/Controls/TimerCard.xaml — change Frame Padding and Label FontSize -->
<Frame Padding="4"          <!-- was: Padding="8" -->
       CornerRadius="12"
       ...>
    ...
    <Label Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2"
           Text="{Binding Elapsed, StringFormat='{0:mm\\:ss}'}"
           FontSize="20"    <!-- was: FontSize="24" -->
           ...>
```

### MetronomePulse circle size reduction
```xml
<!-- Source: AclsTracker/Controls/MetronomePulse.xaml — change Grid and Ellipse sizes -->
<Grid WidthRequest="45" HeightRequest="45">   <!-- was: 50, 50 -->
    <Ellipse x:Name="PulseCircle"
             WidthRequest="40" HeightRequest="40"   <!-- was: 44, 44 -->
             ...>
```

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — no test project in solution |
| Config file | None |
| Quick run command | Manual: deploy to Android emulator, verify no scroll needed |
| Full suite command | Manual: visual inspection on 360dp-width device/emulator |

### Phase Requirements → Test Map

This phase has no formal requirement IDs. All acceptance criteria are visual:

| Behavior | Test Type | How to Verify |
|----------|-----------|---------------|
| ADRENALINA button visible without scroll on standard Android | manual-smoke | Launch app, do not scroll — both drug buttons must be fully visible |
| Rhythm selector fits in one row | manual-smoke | RhythmSelector shows all 5 buttons in single row, no wrap |
| NUEVO CICLO and DEFIBRILAR appear side by side | manual-smoke | Action block row 1 shows both buttons at equal width |
| DataTriggers still work on drug buttons | manual-functional | Trigger IsAdrenalinaSuggested → ADRENALINA turns red |
| BoolToOpacityConverter still works on AMIODARONA | manual-functional | Before first adrenalina dose: AMIODARONA appears dimmed |
| MetrónomoPulse circle animation unaffected | manual-functional | Start metronome — circle pulses normally |
| No clipping on ASISTOLIA button text | manual-smoke | "ASISTOLIA" renders fully on 360dp screen |

### Sampling Rate
- **Per task:** Visual check on Android emulator after each file edit
- **Phase gate:** All 7 behaviors above verified before marking phase complete

### Wave 0 Gaps
None — no test infrastructure needed for pure layout changes. All verification is visual/manual.

---

## Open Questions

1. **MetronomePulse.xaml.cs animation code**
   - What we know: `PulseCircle` is used by name in code-behind for animations
   - What's unclear: Whether animation references absolute pixel values of the circle (44px) that would break if changed to 40px
   - Recommendation: Read MetronomePulse.xaml.cs before editing the XAML — if ScaleAnimation is used (ScaleTo), it's multiplicative and safe; if TranslateTo with hardcoded offsets, verify the math

2. **DefibrilarButton x:Name usage**
   - What we know: The current Button has `x:Name="DefibrilarButton"` (line 99 of MainPage.xaml)
   - What's unclear: Whether MainPage.xaml.cs or any other code accesses this name
   - Recommendation: Search for `DefibrilarButton` in MainPage.xaml.cs before restructuring; move the x:Name to the new button position inside the unified Grid

3. **iOS vs Android rendering of Padding="2,0" on small buttons**
   - What we know: iOS buttons render slightly differently from Android for small Padding values
   - What's unclear: Whether FontSize="11" renders legibly on iOS (different font metrics)
   - Recommendation: Phase targets Android first (per CONTEXT.md "pantallas estándar de Android"); iOS visual parity can be adjusted post-launch if needed

---

## Sources

### Primary (HIGH confidence)
- Direct code read of `AclsTracker/Views/MainPage.xaml` — current layout structure confirmed
- Direct code read of `AclsTracker/Controls/RhythmSelector.xaml` — FlexLayout wrap confirmed
- Direct code read of `AclsTracker/Controls/TimerCard.xaml` — Padding=8, FontSize=24 confirmed
- Direct code read of `AclsTracker/Controls/MetronomePulse.xaml` — Grid size 50/44 confirmed

### Secondary (MEDIUM confidence)
- Vertical space calculation based on standard Android screen dp budget (360×800dp reference)
- MAUI Grid column `*` sizing behavior (from CLAUDE.md project stack knowledge)

### Tertiary (LOW confidence)
- iOS font metric differences for FontSize="11" — unverified, flagged as open question

---

## Metadata

**Confidence breakdown:**
- Layout changes required: HIGH — all files read, all properties confirmed
- Vertical savings estimate: MEDIUM — dp values are estimates; actual rendering varies by device density
- ASISTOLIA fit at FontSize="11": MEDIUM — calculated but unverified on emulator
- Animation safety: MEDIUM — depends on MetronomePulse.xaml.cs content (open question)

**Research date:** 2026-04-04
**Valid until:** Stable — MAUI layout behavior is not fast-moving; valid 90+ days
