# Phase 8: Fix Main UI Layout Issues - Context

**Gathered:** 2026-04-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Three targeted UI fixes on MainPage and related controls: (1) make defibrillation notification banner overlay content instead of taking vertical space, (2) compress the compressions timer card — rename to "T.Comp", remove "EN PAUSA" text, align pause icon with elapsed time, (3) remove "RITMO CARDÍACO" header label and highlight the active rhythm button.

</domain>

<decisions>
## Implementation Decisions

### Banner overlay behavior
- Move NotificationBanner from its own Grid.Row="0" (Auto) to overlay on top of ScrollView content
- Position: top of the content area, floating over INICIAR CODIGO and whatever is below
- Animation: slide down from top + fade in on appear, reverse on dismiss
- Background: fully opaque (change from current 0.95 opacity to 1.0)
- Auto-dismiss: keep existing 8-second timeout, user can also tap ✕
- ZIndex already set to 10 — ensure it stays above all content
- Implementation: put banner and ScrollView in the same Grid row, banner with VerticalOptions="Start"

### Timer card — compressions (T.Comp)
- Rename compressions timer display name to "T.Comp" (Claude decides where — model init or ViewModel)
- Remove "EN PAUSA" label entirely (Row 2, Column 0 in TimerCard.xaml)
- Move pause/resume button from Row 2 to Row 1, right-aligned next to the elapsed time (00:00)
- This change applies ONLY to compressions timer card (Timers[2], ShowPauseButton=True)
- All 6 timer cards should have uniform height — standardize to 3-row layout: Name, Elapsed+icon, Progress bar
- Remove Row 2 from TimerCard Grid RowDefinitions (was "Auto, Auto, Auto, Auto" → "Auto, Auto, Auto")

### Rhythm selector labels
- Remove "RITMO CARDÍACO" section header label (line 10-14 of RhythmSelector.xaml)
- Keep "Ritmo actual: {0}" subtitle — user needs to see current rhythm name
- Add white border highlight on the currently active rhythm button (DataTrigger or binding-based)
- Non-selected buttons keep their current appearance (no border or transparent border)

### Claude's Discretion
- Exact animation duration/easing for banner slide-in
- Where to change the "T.Comp" name (model Name property at init vs display override)
- Border thickness and style for active rhythm button highlight
- Any minor spacing adjustments needed after removing elements

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `NotificationBanner.xaml`: Already has ZIndex="10" and IsVisible binding — just needs repositioning
- `TimerCard.xaml`: 4-row Grid layout — needs Row 2 removal and pause button move to Row 1
- `RhythmSelector.xaml`: VerticalStackLayout with header, subtitle, and Grid of buttons
- `BoolToColorConverter`: Already used for running indicator — could be reused for border color

### Established Patterns
- DataTrigger pattern used extensively (IsOverThreshold on TimerCard, IsSuggested on drug buttons)
- IsVisible bindings for conditional display (IsPaused, ShowPauseButton, IsRunning)
- MVVM with CommunityToolkit ObservableProperty

### Integration Points
- `MainPage.xaml` lines 21-27: Grid RowDefinitions="Auto,*" — change to single row with overlay
- `TimerCard.xaml` lines 19-85: Grid RowDefinitions needs restructuring
- `RhythmSelector.xaml` lines 8-15: Remove header label, add DataTrigger for active button border
- Timer initialization (TimerViewModel or TimerService): Change compressions timer Name to "T.Comp"
- `EventRecordingViewModel`: CurrentRhythm property needed for button highlight binding

</code_context>

<specifics>
## Specific Ideas

- "Necesito ese espacio para que entren los botones adrenalina y amiodarona en la vista" — the banner overlay is specifically to recover vertical space
- "Todos los TimerCard deben ocupar el mismo espacio" — uniform card height is important
- Active rhythm button highlight with white border — quick visual identification in emergency

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 08-fix-main-ui-layout-issues-overlay-defibrillation-banner-compress-timer-cards-remove-cardiac-rhythm-label*
*Context gathered: 2026-04-05*
