---
status: resolved
trigger: "MAUI project fails to compile in Visual Studio with NuGet restore errors and .NET 8.0 EOL framework warnings"
created: 2025-01-24T12:00:00Z
updated: 2025-01-24T14:00:00Z
---

## Current Focus
hypothesis: All build issues resolved - missing Platforms/ directory, missing Resources/Styles, Android API level mismatch
test: Both Android (net9.0-android36.0) and Windows (net9.0-windows10.0.19041.0) builds verified
expecting: User confirms project also builds in Visual Studio
next_action: Await user confirmation that builds work in their environment

## Symptoms
expected: Project compiles successfully in Visual Studio
actual: NuGet restore fails, framework EOL errors (NETSDK1202: net8.0-android/ios/maccatalyst out of support), NETSDK1140: TargetPlatformVersion 1.0 invalid
errors:
- Error al restaurar paquetes NuGet: La operación no se pudo realizar: no se pudieron cargar los detalles del proyecto AclsTracker
- NETSDK1202: La carga de trabajo "net8.0-android" está fuera de soporte
- NETSDK1202: La carga de trabajo "net8.0-maccatalyst" está fuera de soporte
- NETSDK1140: 1.0 no es un valor TargetPlatformVersion válido para maccatalyst
- NETSDK1202: La carga de trabajo "net8.0-ios" está fuera de soporte
- NETSDK1140: 1.0 no es un valor TargetPlatformVersion válido para ios
reproduction: User tries to build project in Visual Studio
timeline: First compilation attempt

## Eliminated

## Evidence
- timestamp: 2025-01-24T12:01:00Z
  checked: Project .csproj file
  found: TargetFrameworks set to "net8.0-android;net8.0-ios;net8.0-maccatalyst" with conditional net8.0-windows10.0.19041.0 for Windows
  implication: Project is targeting .NET 8.0, which is now EOL

- timestamp: 2025-01-24T12:02:00Z
  checked: Installed .NET SDK and workloads
  found: SDK 10.0.201 installed with workloads for iOS (26.2.10217/10.0.100), Android (36.1.30/10.0.100), Mac Catalyst (26.2.10217/10.0.100), and MAUI Windows (10.0.30/10.0.100). Runtimes for .NET 8.0, 9.0, and 10.0 are all installed
  implication: Workloads are installed for .NET 10 SDK, but project references .NET 8.0 workloads which are no longer supported in SDK 10.0

- timestamp: 2025-01-24T12:03:00Z
  checked: .NET MAUI documentation for upgrade path
  found: .NET 8.0 is out of support. Upgrade path is to change TargetFrameworks from net8.0 to net9.0 for all platforms. Documentation confirms net9.0-android, net9.0-ios, net9.0-maccatalyst, net9.0-windows10.0.19041.0 as correct targets
  implication: Fix is to update TargetFrameworks in .csproj from net8.0 to net9.0

- timestamp: 2025-01-24T12:10:00Z
  checked: Applied fix - Updated AclsTracker.csproj
  found: Changed TargetFrameworks from net8.0-android/ios/maccatalyst to net9.0-android/ios/maccatalyst. Added SupportedOSPlatformVersion properties for iOS/Mac Catalyst (15.0). Added explicit PackageReference for Microsoft.Maui.Controls Version 9.0.120
  implication: Project now targets .NET 9.0 which is current LTS and fully supported

- timestamp: 2025-01-24T12:11:00Z
  checked: dotnet restore after fix
  found: Restore completed successfully in 1.47 minutes without any NETSDK1202 or NETSDK1140 errors
  implication: Original framework EOL errors are resolved. Project can now restore NuGet packages for .NET 9.0 target frameworks

- timestamp: 2025-01-24T12:12:00Z
  checked: Build attempts after fix
  found: Windows build fails with "no AppxManifest is specified, but WindowsPackageType is not set to MSIX". Android build fails with missing Android SDK API level 35
  implication: These are NEW platform-specific configuration/environment issues, NOT related to original .NET 8.0 EOL problem. Original NETSDK1202 and NETSDK1140 errors are gone

- timestamp: 2025-01-24T13:01:00Z
  checked: Windows build attempt
  found: Error CS5001: "El programa no contiene ningún método 'Main' estático adecuado para un punto de entrada" - MauiProgram.cs has CreateMauiApp() but no Main() method
  implication: In .NET MAUI 9, the startup pattern changed. Need a Program.cs with Main() that calls MauiProgram.CreateMauiApp().Run()

- timestamp: 2025-01-24T13:02:00Z
  checked: Android build attempt
  found: Error XA5207: "No se ha podido encontrar android.jar para el nivel API 35" - Android SDK API level 35 not installed at C:\Program Files (x86)\Android\android-sdk\platforms\android-35\
  implication: The .NET Android SDK 35.0.105 requires Android API level 35, which is not installed. Options: install API 35 or target a lower API level

- timestamp: 2025-01-24T13:03:00Z
  checked: Available Android SDK platforms
  found: Only android-36 is installed at C:\Program Files (x86)\Android\android-sdk\platforms\android-36\
  implication: We have API 36 but need API 35. Attempting to install API 35 failed with UnauthorizedAccessException (requires admin privileges or different installation method)

- timestamp: 2025-01-24T13:04:00Z
  checked: Attempted to install Android SDK API 35
  found: Installation failed with "Access to the path 'C:\Program Files (x86)\Android\android-sdk\platforms\android-35' is denied" - permissions issue in Program Files directory
  implication: Cannot install API 35 without admin privileges. Better solution: configure project to target API 36 which is already installed

- timestamp: 2025-01-24T14:01:00Z
  checked: Created entire Platforms/ directory structure with platform entry points
  found: Project was missing ALL platform-specific files (Platforms/Android/MainActivity.cs, MainApplication.cs, AndroidManifest.xml; Platforms/Windows/App.xaml + App.xaml.cs; Platforms/iOS and MacCatalyst AppDelegate.cs + Program.cs). These are required by MAUI for platform bootstrapping.
  implication: The CS5001 (no Main entry point) was caused by missing Platforms/Windows/App.xaml.cs which provides the WinUI application entry, and missing Platforms/Android files for Android bootstrap.

- timestamp: 2025-01-24T14:02:00Z
  checked: Changed Android TFM from net9.0-android to net9.0-android36.0 in .csproj
  found: Explicit API 36 in TFM resolves XA5207 since only android-36 SDK platform is installed. Removed obsolete TargetFrameworkVersion property. Added SupportedOSPlatformVersion for Android (21.0) and Windows (10.0.17763.0).
  implication: Android build now finds android.jar at correct API level 36

- timestamp: 2025-01-24T14:03:00Z
  checked: Created missing Resources/Styles/Colors.xaml and Styles.xaml
  found: App.xaml references these resource dictionaries but they did not exist, causing XamlC error XC0124. Created standard MAUI template files with emergency/medical app color palette.
  implication: XAML resource resolution now works

- timestamp: 2025-01-24T14:04:00Z
  checked: Simplified AndroidManifest.xml to remove icon references
  found: Original manifest referenced mipmap/appicon and mipmap/appicon_round which don't exist (no app icon resources generated yet). Removed icon/roundIcon attributes to let MAUI handle defaults.
  implication: Eliminates APT2260 resource-not-found errors

- timestamp: 2025-01-24T14:05:00Z
  checked: Final build verification - Android target
  found: "dotnet build -f net9.0-android36.0" completed successfully. Output: "Compilación correcta. 0 Advertencia(s) 0 Errores" with DLL at bin/Debug/net9.0-android36.0/AclsTracker.dll
  implication: Android build fully working

- timestamp: 2025-01-24T14:06:00Z
  checked: Final build verification - Windows target
  found: "dotnet build -f net9.0-windows10.0.19041.0" completed successfully. Output: "Compilación correcta. 0 Errores" (24 XC0022 binding warnings only) with DLL at bin/Debug/net9.0-windows10.0.19041.0/win10-x64/AclsTracker.dll
  implication: Windows build fully working. CS5001 entry point error resolved by Platforms/Windows/App.xaml.cs

## Resolution
root_cause: Multiple cascading issues from incomplete .NET 8 to .NET 9 MAUI migration:
  1. Project targeted .NET 8.0 (EOL) causing NETSDK1202/NETSDK1140 errors
  2. Missing entire Platforms/ directory (Android, Windows, iOS, MacCatalyst entry points) causing CS5001 no Main() entry point
  3. Android TFM defaulted to API 35 but only API 36 SDK was installed, causing XA5207
  4. Missing Resources/Styles/Colors.xaml and Styles.xaml causing XC0124 XAML errors
  5. AndroidManifest.xml referenced non-existent mipmap icons causing APT2260 errors
fix: Complete project structure restoration and configuration:
  - Changed TargetFrameworks to net9.0-android36.0;net9.0-ios;net9.0-maccatalyst (+ conditional Windows)
  - Created Platforms/Android/ (MainActivity.cs, MainApplication.cs, AndroidManifest.xml)
  - Created Platforms/Windows/ (App.xaml, App.xaml.cs) with MauiWinUIApplication entry
  - Created Platforms/iOS/ (AppDelegate.cs, Program.cs)
  - Created Platforms/MacCatalyst/ (AppDelegate.cs, Program.cs)
  - Created Resources/Styles/Colors.xaml and Styles.xaml with medical app color palette
  - Added SupportedOSPlatformVersion for Android (21.0) and Windows (10.0.17763.0)
  - WindowsPackageType set to None for unpackaged deployment
  - App.xaml.cs overrides CreateWindow (MAUI 9 pattern)
verification: Both Android and Windows targets compile successfully with 0 errors.
  - Android: "dotnet build -f net9.0-android36.0" -> Compilación correcta, 0 errors, 0 warnings
  - Windows: "dotnet build -f net9.0-windows10.0.19041.0" -> Compilación correcta, 0 errors, 24 XC0022 warnings (non-blocking)
files_changed:
  - AclsTracker\AclsTracker.csproj
  - AclsTracker\App.xaml.cs
  - AclsTracker\Platforms\Android\MainActivity.cs (new)
  - AclsTracker\Platforms\Android\MainApplication.cs (new)
  - AclsTracker\Platforms\Android\AndroidManifest.xml (new)
  - AclsTracker\Platforms\Windows\App.xaml (new)
  - AclsTracker\Platforms\Windows\App.xaml.cs (new)
  - AclsTracker\Platforms\iOS\AppDelegate.cs (new)
  - AclsTracker\Platforms\iOS\Program.cs (new)
  - AclsTracker\Platforms\MacCatalyst\AppDelegate.cs (new)
  - AclsTracker\Platforms\MacCatalyst\Program.cs (new)
  - AclsTracker\Resources\Styles\Colors.xaml (new)
  - AclsTracker\Resources\Styles\Styles.xaml (new)
