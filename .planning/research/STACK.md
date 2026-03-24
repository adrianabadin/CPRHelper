# Stack Research

**Domain:** Mobile Medical ACLS Application
**Researched:** 24/03/2026
**Confidence:** HIGH

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| **.NET MAUI** | .NET 8+ / MAUI Latest | Cross-platform mobile framework | Native compilation to native binaries provides near-native performance critical for real-time metronome and emergency response timing. Built-in security and HIPAA compliance features. Desktop support (Windows/macOS) for clinical workstations. Strong integration with .NET/Azure backend ecosystem. |
| **SQLite** | Latest stable | Embedded local database | Industry standard for offline-first mobile apps. Supports encrypted storage. Lightweight and reliable for device-side data persistence during emergencies. |
| **Syncfusion UI Components** | Latest | UI component library | Professional-grade UI components optimized for cross-platform development. Includes charts, timers, and data visualization components useful for ACLS apps. Reduces development time and ensures consistent UX across platforms. |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| **Plugin.Maui.Audio** | Latest | Audio metronome playback | For precise audio timing of CPR compression rate (100-120 BPM). Cross-platform audio abstraction. |
| **CommunityToolkit.Maui** | Latest | Performance optimizations | Improves rendering performance for smooth metronome animations and timer updates. |
| **Microsoft.Data.Sqlite** | Latest | Database access | Modern SQLite API for secure, encrypted local data storage. |
| **iTextSharp / QuestPDF** | Latest | PDF export | For generating ACLS event logs in PDF format for clinical documentation. |
| **CsvHelper** | Latest | CSV export | For structured data export for analysis and record keeping. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| **Visual Studio 2022+** | Primary IDE | Full .NET MAUI support, hot reload, debugger integration. Use .NET 8 SDK. |
| **Android Emulator + iOS Simulator** | Device testing | Test on both platforms simultaneously. Essential for verifying audio timing accuracy on different OS audio stacks. |
| **Azure Storage (optional)** | Cloud sync backend | If cloud sync is added later, Azure Storage provides HIPAA-compliant BAAs. |

## Installation

```bash
# Core
dotnet add package Microsoft.Maui.Controls
dotnet add package Microsoft.Data.Sqlite

# Supporting
dotnet add package Plugin.Maui.Audio
dotnet add package CommunityToolkit.Maui
dotnet add package iTextSharp
dotnet add package CsvHelper

# Dev dependencies
dotnet add package Microsoft.Maui.Controls.Compatibility
```

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| .NET MAUI | React Native | If team has strong JavaScript/React expertise and needs faster time-to-market with extensive third-party library ecosystem. However, React Native may have slight performance edge for non-critical features but lacks native compilation advantage for real-time timing accuracy. |
| .NET MAUI | Flutter | If team wants Dart-based development. Flutter offers excellent performance but smaller ecosystem compared to .NET. |
| SQLite | Realm / Couchbase Lite | If need advanced sync capabilities out of the box. However, SQLite is simpler and more than sufficient for ACLS event logging. |
| Azure Storage | AWS / Google Cloud | If organization already has AWS/GCP expertise. All major cloud providers offer HIPAA-compliant options. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| Online-only APIs | App must work during emergencies when connectivity is unreliable | Offline-first architecture with local SQLite storage |
| Simple JSON storage for logs | Not suitable for querying or export generation | SQLite for structured data with encryption |
| Platform-specific audio stacks | Increases code complexity, difficult to maintain timing consistency | Plugin.Maui.Audio for cross-platform audio abstraction |
| WebView-based UI | Poor performance, difficult to optimize animations | Native MAUI controls with CommunityToolkit optimizations |

## Stack Patterns by Variant

**If prioritizing maximum performance and timing accuracy:**
- Use .NET MAUI with native audio plugin
- Because Native compilation ensures precise metronome timing, critical for CPR guidance
- Use CommunityToolkit.Maui for 60fps animations

**If prioritizing rapid development with extensive libraries:**
- Use React Native instead
- Because Larger ecosystem provides more pre-built components, trading slight performance edge for speed

**If adding cloud sync later:**
- Use Azure Storage or AWS S3 with HIPAA BAA
- Because Both providers offer compliant infrastructure with clear security practices

## Version Compatibility

| Package A | Compatible With | Notes |
|-----------|-----------------|-------|
| .NET MAUI 8.0 | .NET 8 SDK | Use Visual Studio 2022+ with .NET 8 workload |
| Plugin.Maui.Audio | MAUI 8.0 | Verify timing accuracy across platforms (iOS vs Android audio stacks may differ slightly) |
| Microsoft.Data.Sqlite | MAUI 8.0 | Supports encryption at rest, essential for PHI protection |

## Sources

- Google Search — ".NET MAUI vs React Native medical app 2024"
- Google Search — "offline-first mobile medical app architecture"
- Research on .NET MAUI performance and security features
- AHA ACLS App feature analysis

---
*Stack research for: Mobile Medical ACLS Application*
*Researched: 24/03/2026*