<!-- GSD:project-start source:PROJECT.md -->
## Project

**ACLS Tracker**

Aplicación móvil multiplataforma (Android e iOS) para seguimiento y guía de Reanimación Cardiopulmonar Avanzada (ACLS) según normas de la American Heart Association (AHA) edición 2020. Diseñada para uso tanto en entrenamiento de equipos como en emergencias reales de reanimación, proporciona un metrónomo con guía visual y auditiva para compresiones torácicas, recordatorios protocolizados basados en tiempo, y registro completo de eventos durante el código.

**Core Value:** El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales, mejorando la adherencia al protocolo AHA ACLS 2020.

### Constraints

- **Framework**: .NET MAUI o React Native — debe seleccionarse uno al iniciar desarrollo
- **Plataformas**: Android e iOS — ambas requeridas
- **Conectividad**: Funcionalidad offline principal — no debe depender de conexión durante emergencia
- **Protocolo**: AHA ACLS 2020 — debe seguir las guías actualizadas de 2020 estrictamente
- **UI/UX**: Botones en pantalla — interfaz táctil optimizada para uso rápido en emergencias
<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->
## Technology Stack

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
# Core
# Supporting
# Dev dependencies
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
- Use .NET MAUI with native audio plugin
- Because Native compilation ensures precise metronome timing, critical for CPR guidance
- Use CommunityToolkit.Maui for 60fps animations
- Use React Native instead
- Because Larger ecosystem provides more pre-built components, trading slight performance edge for speed
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
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
