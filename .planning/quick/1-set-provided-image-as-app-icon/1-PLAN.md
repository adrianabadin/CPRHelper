---
phase: quick
plan: 1
type: execute
wave: 1
depends_on: []
files_modified:
  - AclsTracker/Resources/AppIcon/appicon.png
  - AclsTracker/AclsTracker.csproj
autonomous: false
requirements: [QUICK-ICON-01]

must_haves:
  truths:
    - "The app icon shown on device home screen and app drawer is the CPR image (blue background, red heart, CPR hands, heartbeat line)"
    - "The icon displays correctly on Android with adaptive icon support (no clipping on rounded launchers)"
  artifacts:
    - path: "AclsTracker/Resources/AppIcon/appicon.png"
      provides: "Source icon image MAUI uses to generate all platform-specific sizes"
    - path: "AclsTracker/AclsTracker.csproj"
      provides: "MauiIcon declaration pointing to Resources/AppIcon/appicon.png"
  key_links:
    - from: "AclsTracker/AclsTracker.csproj"
      to: "AclsTracker/Resources/AppIcon/appicon.png"
      via: "MauiIcon Include= declaration"
      pattern: "<MauiIcon.*appicon"
---

<objective>
Set the CPR/ACLS image as the app icon for the AclsTracker MAUI application.

Purpose: Replace the default (empty) app icon with the provided CPR image — blue background, red heart, hands performing CPR, heartbeat line — making the app instantly identifiable on device home screens.
Output: appicon.png placed in Resources/AppIcon/, MauiIcon configured in .csproj, icon visible on Android and iOS after rebuild.
</objective>

<execution_context>
@C:/Users/Adria/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/Adria/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/STATE.md

Source image: C:\Users\Adria\Downloads\Gemini_Generated_Image_ybeqjmybeqjmybeq.png

In .NET MAUI, a single source image in Resources/AppIcon/ is declared with <MauiIcon> in the .csproj. The build toolchain generates all platform sizes automatically (Android mipmap-* folders, iOS Assets.xcassets). No manual resizing is needed. The image should be at least 1024x1024px for best results.

The .csproj currently has no <MauiIcon> entry and Resources/AppIcon/ is empty.
</context>

<tasks>

<task type="auto">
  <name>Task 1: Copy source image to AppIcon and configure project</name>
  <files>
    AclsTracker/Resources/AppIcon/appicon.png
    AclsTracker/AclsTracker.csproj
  </files>
  <action>
    Step 1 — Copy the source image:
    Copy C:\Users\Adria\Downloads\Gemini_Generated_Image_ybeqjmybeqjmybeq.png to AclsTracker/Resources/AppIcon/appicon.png

    Step 2 — Add MauiIcon to AclsTracker.csproj:
    Inside the first <ItemGroup> that contains <MauiAsset>, add (or create a new ItemGroup with):

    ```xml
    <MauiIcon Include="Resources\AppIcon\appicon.png" />
    ```

    Do NOT add ForegroundFile or Color attributes — the image already has its own background baked in, so a plain MauiIcon declaration is correct. Adding Color would overlay a tinted background on Android adaptive icons.

    Step 3 — Verify the file was copied and the .csproj has the MauiIcon line by reading both files.
  </action>
  <verify>
    <automated>
      ls -la /c/Users/Adria/Documents/code/CPRHelper/AclsTracker/Resources/AppIcon/appicon.png
      grep "MauiIcon" /c/Users/Adria/Documents/code/CPRHelper/AclsTracker/AclsTracker.csproj
    </automated>
  </verify>
  <done>
    appicon.png exists in Resources/AppIcon/ and AclsTracker.csproj contains a &lt;MauiIcon Include="Resources\AppIcon\appicon.png" /&gt; line.
  </done>
</task>

<task type="checkpoint:human-verify" gate="blocking">
  <what-built>Copied CPR image to Resources/AppIcon/appicon.png and added MauiIcon declaration to .csproj. The MAUI build toolchain will auto-generate all Android and iOS icon sizes from this source on next build.</what-built>
  <how-to-verify>
    1. Build and deploy to Android emulator or device: run the app from Visual Studio 2022 targeting Android.
    2. Press Home to exit to the launcher.
    3. Confirm the app icon in the app drawer shows the CPR image (blue background, red heart, hands doing CPR, heartbeat line) — NOT the default MAUI dotnet bot icon.
    4. If the icon still shows the old icon, do a clean build: Build > Clean Solution, then Build > Rebuild Solution, then deploy again.
  </how-to-verify>
  <resume-signal>Type "approved" if the icon looks correct, or describe what you see if it is wrong.</resume-signal>
</task>

</tasks>

<verification>
- AclsTracker/Resources/AppIcon/appicon.png exists and is a valid PNG
- AclsTracker.csproj contains `<MauiIcon Include="Resources\AppIcon\appicon.png" />`
- App builds without errors related to icon resources
- Device/emulator home screen shows the CPR image as the app icon
</verification>

<success_criteria>
The AclsTracker app icon on Android (and iOS after build targeting iOS) displays the provided CPR image — blue background, red heart, hands doing CPR, heartbeat line. The icon is correctly generated at all required platform sizes by the MAUI build toolchain.
</success_criteria>

<output>
After completion, create .planning/quick/1-set-provided-image-as-app-icon/1-SUMMARY.md with what was done, files changed, and any issues encountered.
</output>
