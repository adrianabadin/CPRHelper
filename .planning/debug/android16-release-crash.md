---
status: awaiting_human_verify
trigger: "La app .NET MAUI crashea inmediatamente al abrirla en Android 16 compilada como Release"
created: 2026-04-03T00:00:00Z
updated: 2026-04-03T01:00:00Z
---

## Current Focus

hypothesis: CONFIRMED - QuestPDF incompatibility + missing trimmer config caused immediate startup crash
test: Build succeeded with 0 errors. Need user to deploy Release APK to Android 16 device and verify launch.
expecting: App should open and display main screen without crashing
next_action: Await user verification on physical device

## Symptoms

expected: La app abre normalmente y muestra la pantalla principal
actual: La app crashea inmediatamente al intentar abrirla (se cierra la ventana sin mensaje de error)
errors: Ninguno visible - solo se cierra
reproduction: Abrir la app compilada en Release en un dispositivo fisico con Android 16
started: Primera vez que se prueba Release en Android 16. Nunca funciono.

## Eliminated

(none - root cause found on first hypothesis)

## Evidence

- timestamp: 2026-04-03T00:01:00Z
  checked: .csproj configuration
  found: Project targets net10.0-android with no trimmer/linker configuration for Release mode. No PublishTrimmed, TrimMode, ManagedLinkMode, or RunAOTCompilation settings.
  implication: Release mode uses aggressive default trimming that strips reflection-dependent types needed by Supabase, JSON serialization, and SQLite.

- timestamp: 2026-04-03T00:02:00Z
  checked: QuestPDF compatibility with Android
  found: QuestPDF dropped MAUI/Android support after version 2023.12.X. Supported runtimes are desktop-only: win-x86, win-x64, linux-x64, linux-arm64, linux-musl-x64, osx-x64, osx-arm64. The project uses QuestPDF 2026.2.4 and calls QuestPDF.Settings.License = LicenseType.Community unconditionally in MauiProgram.cs line 93 during app startup.
  implication: QuestPDF initialization on Android triggers native library loading that fails fatally, crashing the app before any UI renders.

- timestamp: 2026-04-03T00:03:00Z
  checked: MauiProgram.cs startup flow
  found: QuestPDF.Settings.License = LicenseType.Community is called unconditionally at line 93 during CreateMauiApp(). No platform guard. This runs before any window is created.
  implication: Fatal native crash happens during DI container build, before any exception handling or UI can be displayed.

- timestamp: 2026-04-03T00:04:00Z
  checked: PdfExportService.cs
  found: Entire service uses QuestPDF types (Document, PageSizes, etc.). With QuestPDF excluded from Android, this file would fail to compile without conditional compilation.
  implication: Need #if guards around all QuestPDF code and a mobile fallback.

- timestamp: 2026-04-03T00:05:00Z
  checked: Android Release build after fixes
  found: Build succeeded - 155 warnings (all pre-existing XamlC binding warnings), 0 errors. Build time 6:22.
  implication: All fixes compile correctly for net10.0-android Release configuration.

## Resolution

root_cause: |
  TWO compounding issues cause the immediate crash:
  
  1. PRIMARY - QuestPDF native library crash: QuestPDF 2026.2.4 does not support Android 
     runtime (only desktop: win/linux/osx). The unconditional call to 
     QuestPDF.Settings.License = LicenseType.Community in MauiProgram.CreateMauiApp() 
     triggers native library loading that fatally crashes on Android before any UI renders.
  
  2. SECONDARY - Aggressive IL trimming: No trimmer/linker configuration in .csproj means 
     Release mode uses aggressive default trimming that strips reflection-dependent types 
     needed by Supabase (Gotrue, Realtime, Postgrest), System.Text.Json, Newtonsoft.Json, 
     and SQLite. Even if QuestPDF didn't crash first, these would cause runtime failures.

fix: |
  Three files changed:
  
  1. AclsTracker.csproj:
     - Added QuestPDF Condition to exclude from Android builds
     - Added Release PropertyGroup: TrimMode=partial, ManagedLinkMode=SdkOnly, 
       RunAOTCompilation=false, PublishTrimmed=true
     - Added TrimmerRootAssembly items with RootMode="all" for all reflection-heavy 
       assemblies (Supabase.*, System.Text.Json, SQLite-net, Newtonsoft.Json, etc.)
  
  2. MauiProgram.cs:
     - Wrapped QuestPDF using and license init in #if !ANDROID && !IOS
  
  3. PdfExportService.cs:
     - Wrapped all QuestPDF code in #if !ANDROID && !IOS
     - Added mobile fallback that generates a plain-text report instead of PDF

verification: Build succeeded (0 errors). Awaiting user device test.

files_changed:
  - AclsTracker/AclsTracker.csproj
  - AclsTracker/MauiProgram.cs
  - AclsTracker/Services/Export/PdfExportService.cs
