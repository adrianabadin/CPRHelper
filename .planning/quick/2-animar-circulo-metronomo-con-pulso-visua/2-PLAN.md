---
phase: quick
plan: 2
type: execute
wave: 1
depends_on: []
files_modified:
  - AclsTracker/Controls/MetronomePulse.xaml
  - AclsTracker/Controls/MetronomePulse.xaml.cs
autonomous: true
requirements: []
must_haves:
  truths:
    - "The metronome circle visually grows and shrinks like a heartbeat on each beat"
    - "The pulse animation is synchronized with the audio metronome tick"
    - "The animation feels like a heartbeat — double-pump systole/diastole pattern"
  artifacts:
    - path: "AclsTracker/Controls/MetronomePulse.xaml"
      provides: "Pulse circle visual element"
    - path: "AclsTracker/Controls/MetronomePulse.xaml.cs"
      provides: "Heartbeat animation logic"
  key_links:
    - from: "MetronomeViewModel.BeatPulse"
      to: "MetronomePulse.IsPulsing"
      via: "BindableProperty change triggers AnimatePulse"
      pattern: "OnIsPulsingChanged.*AnimatePulse"
---

<objective>
Enhance the metronome pulse circle to animate with a realistic heartbeat effect — the circle should expand and contract with a double-pump (lub-dub) pattern synchronized to each beat tick, providing clear visual feedback of the CPR compression rhythm.

Purpose: Give the code leader a strong visual cue that reinforces the audio metronome, improving rhythm adherence during CPR.
Output: Updated MetronomePulse control with heartbeat-style scale animation.
</objective>

<execution_context>
@C:/Users/Adria/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/Adria/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@AclsTracker/Controls/MetronomePulse.xaml
@AclsTracker/Controls/MetronomePulse.xaml.cs
@AclsTracker/ViewModels/MetronomeViewModel.cs
</context>

<tasks>

<task type="auto">
  <name>Task 1: Implement heartbeat pulse animation on MetronomePulse circle</name>
  <files>AclsTracker/Controls/MetronomePulse.xaml, AclsTracker/Controls/MetronomePulse.xaml.cs</files>
  <action>
Update the AnimatePulse method in MetronomePulse.xaml.cs to produce a heartbeat "lub-dub" effect instead of the current single scale pulse. The animation should:

1. Replace the current simple scale (1.0 -> 1.15 -> 1.0 in 200ms) with a double-pump heartbeat:
   - First pump (systole "lub"): ScaleTo 1.35 over 80ms with CubicOut easing
   - Quick partial relax: ScaleTo 1.1 over 60ms with CubicIn easing
   - Second pump (diastole "dub"): ScaleTo 1.25 over 70ms with CubicOut easing  
   - Full relax back: ScaleTo 1.0 over 120ms with CubicIn easing
   - Total: ~330ms which fits within the 500ms interval at 120 BPM

2. Also add an opacity pulse alongside the scale for extra visual impact:
   - Start at Opacity 0.8 (current resting value)
   - On first pump: FadeTo 1.0 over 80ms
   - Keep full opacity through second pump
   - Fade back to 0.8 over 120ms during the relax phase
   - Use Task.WhenAll to run opacity and scale animations in parallel where appropriate

3. In the XAML file, increase the circle size slightly for better visibility of the animation:
   - Change PulseCircle WidthRequest from 40 to 42
   - Change PulseCircle HeightRequest from 40 to 42
   - Keep the Grid container at 45x45 (enough room for the 1.35x scale peak: 42 * 1.35 = ~57px visual, but scale animation uses transform so it won't affect layout)

4. Make sure CancelAnimations() is called at the start of AnimatePulse to cancel any in-progress animation before starting a new beat.

Do NOT change the IsPulsing BindableProperty mechanism or the OnIsPulsingChanged handler — the trigger mechanism from MetronomeViewModel.BeatPulse toggling is correct and should remain as-is.
  </action>
  <verify>
    <automated>cd C:/Users/Adria/Documents/code/CPRHelper && dotnet build AclsTracker/AclsTracker.csproj --no-restore -v q 2>&amp;1 | tail -5</automated>
  </verify>
  <done>The MetronomePulse circle animates with a heartbeat double-pump pattern (lub-dub) on each beat, with combined scale and opacity animation totaling ~330ms, fitting within the 500ms minimum beat interval at 120 BPM. The project compiles without errors.</done>
</task>

</tasks>

<verification>
- Project builds successfully: `dotnet build AclsTracker/AclsTracker.csproj`
- MetronomePulse.xaml.cs contains double-pump animation (two ScaleTo calls expanding, not just one)
- Animation total duration is under 450ms (safe margin for 120 BPM = 500ms interval)
- PulseCircle includes opacity animation alongside scale
</verification>

<success_criteria>
The metronome circle visibly grows and shrinks with a heartbeat-like double-pump pattern on each beat, synchronized with the audio tick. The animation is smooth and completes well within the beat interval.
</success_criteria>

<output>
After completion, create `.planning/quick/2-animar-circulo-metronomo-con-pulso-visua/2-SUMMARY.md`
</output>
