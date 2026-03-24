# Architecture Research

**Domain:** Mobile Medical ACLS Application
**Researched:** 24/03/2026
**Confidence:** HIGH

## Standard Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                    │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────┐  ┌─────────┐  ┌─────────┐        │
│  │ Metron  │  │  Timer  │  │   UI    │        │
│  │ Screen  │  │ Manager │  │  Pages  │        │
│  └────┬────┘  └────┬────┘  └────┬────┘        │
│       │            │            │              │
├───────┴────────────┴────────────┴──────────────┤
│                    BUSINESS LOGIC LAYER                  │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────┐   │
│  │          Domain Services & Logic             │   │
│  │  - Metronome Engine (100-120 BPM)       │   │
│  │  - Timer Coordinator (2-min cycles)       │   │
│  │  - Medication Scheduler (protocol logic)   │   │
│  │  - Reminder Engine (context-aware)         │   │
│  │  - Session Logger (event capture)         │   │
│  └──────────────────────────────────────────────────┘   │
│              ↓                                     │
├─────────────────────────────────────────────────────────────┤
│                     DATA ACCESS LAYER                  │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ Repository│  │ Repository│  │ Repository│       │
│  │: Sessions │  │: Events  │  │: Config  │       │
│  └──────────┘  └──────────┘  └──────────┘       │
├─────────────────────────────────────────────────────────────┤
│                    DATA STORAGE LAYER                   │
│  ┌──────────────────────────────────────────────┐    │
│  │         SQLite Database (Encrypted)          │    │
│  │  - Sessions                            │    │
│  │  - Events (medications, rhythms, etc.)   │    │
│  │  - Config (metronome settings, reminders) │    │
│  └──────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
           (optional sync when connected)
                  ↓
         ┌────────────────┐
         │  Cloud API   │
         │ (HIPAA-compliant)
         └────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|------------------------|
| **Metronome Engine** | Manages audio/visual timing at 100-120 BPM. Handles play/pause/rate changes. | Plugin.Maui.Audio with high-precision timer using Stopwatch. Visual update via CommunityToolkit.Maui for 60fps. |
| **Timer Coordinator** | Orchestrates all timers (2-min cycles, medication countdowns). Coordinates alerts. | TimerService with ObservableCollection<Timer> for UI binding. Background task for offline operation. |
| **Medication Scheduler** | Implements AHA ACLS 2020 medication protocols. When to remind based on rhythm and elapsed time. | State machine per rhythm type (VF/VT, AEA, Asystole, etc.). Rules engine for timing. |
| **Reminder Engine** | Generates context-aware audio prompts ("Time for epinephrine?"). Speech synthesis or pre-recorded audio. | Text-to-Speech (TTS) plugin or pre-recorded clips. Queue system for non-blocking playback. |
| **Session Logger** | Captures all user actions with timestamps. Enables PDF/CSV export. | Repository pattern with WriteSessionEvent(). Auto-timestamp on insert. |
| **SQLite Database** | Stores sessions, events, configuration. Encrypted at rest. | Microsoft.Data.Sqlite with connection encryption. Schema normalization (Sessions → Events). |
| **Export Service** | Generates PDF reports and CSV data from session logs. | iTextSharp/QuestPDF for PDF. CsvHelper for CSV. Async operation to avoid UI freeze. |

## Recommended Project Structure

```
src/
├── App/
│   ├── App.xaml                 # App entry point
│   ├── AppShell.xaml          # Navigation shell
│   ├── Models/                # Data models
│   │   ├── Session.cs
│   │   ├── SessionEvent.cs
│   │   └── MedicationProtocol.cs
│   ├── ViewModels/            # MVVM view models
│   │   ├── MainViewModel.cs
│   │   ├── MetronomeViewModel.cs
│   │   ├── TimerViewModel.cs
│   │   └── SessionViewModel.cs
│   ├── Views/                 # MAUI XAML pages
│   │   ├── MainPage.xaml
│   │   ├── MetronomePage.xaml
│   │   ├── TimerPage.xaml
│   │   └── SessionHistoryPage.xaml
│   ├── Services/              # Business logic layer
│   │   ├── Audio/
│   │   │   ├── IMetronomeAudio.cs
│   │   │   └── MetronomeAudioService.cs
│   │   ├── Timer/
│   │   │   ├── ITimerCoordinator.cs
│   │   │   └── TimerCoordinatorService.cs
│   │   ├── Protocol/
│   │   │   ├── IMedicationScheduler.cs
│   │   │   └── ACLS2020MedicationScheduler.cs
│   │   ├── Reminder/
│   │   │   ├── IReminderEngine.cs
│   │   │   └── ContextAwareReminderEngine.cs
│   │   ├── Data/
│   │   │   ├── IDatabase.cs
│   │   │   ├── SqliteDatabase.cs
│   │   │   ├── ISessionRepository.cs
│   │   │   ├── SessionRepository.cs
│   │   │   └── EventRepository.cs
│   │   └── Export/
│   │       ├── IPdfExporter.cs
│   │       ├── PdfExporterService.cs
│   │       ├── ICsvExporter.cs
│   │       └── CsvExporterService.cs
│   ├── Converters/           # XAML value converters
│   ├── Controls/             # Custom UI controls
│   │   ├── MetronomeVisualizer.xaml
│   │   └── TimerDisplay.xaml
│   └── Resources/            # Strings, styles, images
├── Data/
│   ├── Migrations/           # SQLite schema migrations
│   └── SeedData/            # Initial protocol data
└── Platforms/
    ├── Android/
    │   └── MainApplication.cs
    ├── iOS/
    │   └── AppDelegate.cs
    └── Windows/
        └── MainWindow.cs
```

### Structure Rationale

- **App/Models/** — POCO classes representing domain entities. Separate from ViewModels for clean separation.
- **App/ViewModels/** — MVVM pattern. Enables UI testing and separation of concerns.
- **App/Views/** — MAUI XAML pages. Minimal code-behind, binding to ViewModels.
- **App/Services/** — Business logic grouped by domain (Audio, Timer, Protocol, Data, Export). Facilitates testing and future extension.
- **App/Data/** — Database-specific code. Migrations ensure schema evolution without data loss.
- **Platforms/** — Platform-specific entry points. Only necessary for MAUI lifecycle hooks.

## Architectural Patterns

### Pattern 1: MVVM (Model-View-ViewModel)

**What:** Separation of UI (View) from logic (ViewModel), with Models representing data.
**When to use:** Always in MAUI apps. Enables unit testing of business logic without UI dependencies.
**Trade-offs:** More boilerplate code than code-behind, but testability and maintainability gains outweigh cost.

**Example:**
```csharp
// ViewModel (business logic)
public class MetronomeViewModel : ObservableObject
{
    private int _beatsPerMinute;
    public int BeatsPerMinute
    {
        get => _beatsPerMinute;
        set => SetProperty(ref _beatsPerMinute, value);
    }

    public ICommand ToggleMetronomeCommand { get; }
}

// View (XAML)
<Label Text="{Binding BeatsPerMinute}" />
<Button Command="{Binding ToggleMetronomeCommand}" />
```

### Pattern 2: Repository Pattern

**What:** Abstraction over data access. Interfaces define contracts, implementations use SQLite.
**When to use:** When database access is needed. Enables swapping SQLite for testing or future cloud storage.
**Trade-offs:** Adds abstraction layer, but decouples business logic from storage technology.

**Example:**
```csharp
// Interface
public interface ISessionRepository
{
    Task<Session> GetAsync(int id);
    Task<int> AddAsync(Session session);
}

// Implementation
public class SessionRepository : ISessionRepository
{
    private readonly SqliteDatabase _db;
    public async Task<int> AddAsync(Session session)
    {
        return await _db.InsertAsync(session);
    }
}
```

### Pattern 3: Dependency Injection

**What:** MAUI built-in DI container registers services. Views resolve ViewModels, which resolve services.
**When to use:** For all services. MAUI strongly encourages DI.
**Trade-offs:** Initial setup complexity, but resolves circular dependencies and enables mocking.

**Example:**
```csharp
// App.xaml.cs
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.Services.AddSingleton<SqliteDatabase>();
    builder.Services.AddTransient<ISessionRepository, SessionRepository>();
    builder.Services.AddTransient<MetronomeViewModel>();
    return builder.Build();
}

// Constructor injection
public MetronomeViewModel(ISessionRepository repo) { ... }
```

## Data Flow

### Request Flow

```
[User Taps "Start CPR"]
    ↓
[MetronomeViewModel.StartCommand.Execute()]
    ↓
[TimerCoordinatorService.StartCprCycle()]
    ↓
[MetronomeAudioService.Play(110 BPM)]
    ↓
[MetronomeVisualizer.AnimateVisual()] ← UI binding
    ↓
[SessionRepository.LogEvent(CPR_START)]
    ↓
[SqliteDatabase.Insert()]
```

### State Management

```
[ObservableCollection<TimerEvent>]
    ↓ (subscribe)
[Timer UI Views] ←→ [TimerCoordinatorService] → [SessionLogger]
    ↓
[SQLite Database]
```

### Key Data Flows

1. **Metronome Control Flow:** User toggles → ViewModel command → Audio service play/pause → Visualizer updates via data binding.
2. **Session Recording Flow:** Every action → SessionLogger captures event → Repository writes to SQLite with timestamp.
3. **Export Flow:** User taps export → ExportService queries SQLite → Generates PDF/CSV → Saves to device file system.

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 0-1k users | Single-device SQLite is sufficient. No backend needed. |
| 1k-100k users | Add cloud sync (optional) for backup and cross-device access. Azure Storage with HIPAA BAA. |
| 100k+ users | Consider multi-region cloud deployment, load balancing for sync API. Not needed for MVP. |

### Scaling Priorities

1. **First bottleneck:** SQLite performance with large session history (1000+ sessions). → Add pagination, lazy loading for history page.
2. **Second bottleneck:** Export generation for very long sessions (1+ hours). → Background async export to avoid UI freeze.

## Anti-Patterns

### Anti-Pattern 1: Code-Behind Logic

**What people do:** Put business logic in XAML code-behind files (e.g., Main.xaml.cs).
**Why it's wrong:** Code-behind is tightly coupled to UI, cannot be unit tested. Hard to maintain.
**Do this instead:** MVVM pattern. Move logic to ViewModel, use data binding.

### Anti-Pattern 2: Direct SQLite Access Everywhere

**What people do:** Every ViewModel opens SQLiteConnection directly.
**Why it's wrong:** Duplicate connection logic. Cannot mock for testing. Violates single responsibility.
**Do this instead:** Repository pattern with dependency injection. Single Db instance injected.

### Anti-Pattern 3: Blocking UI with Export

**What people do:** Generate PDF synchronously on button tap. UI freezes.
**Why it's wrong:** Bad UX. User thinks app crashed.
**Do this instead:** Async/await with progress indicator. BackgroundTask API.

## Integration Points

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| (Future) Cloud API | REST/HTTPS with TLS 1.3+ | When adding sync. Must use HIPAA-compliant provider with BAA. |
| (Future) ECG Device | Bluetooth LE (BLE) | Optional v2+ feature. iOS requires CoreBluetooth, Android uses BluetoothManager. |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| UI Layer ↔ Business Logic | Data binding / Commands | MVVM standard. No direct SQLite access from ViewModels. |
| Business Logic ↔ Data Layer | Repository interfaces | Enables mocking for unit tests. |
| Offline ↔ Sync | Background task with network check | Do not block UI while syncing. Handle sync conflicts. |

## Sources

- Google Search — "offline-first mobile medical app architecture"
- Google Search — "HIPAA compliant data storage mobile apps"
- .NET MAUI documentation (dependency injection, MVVM)
- SQLite best practices for mobile apps
- AHA ACLS 2020 Guidelines (protocol requirements)

---
*Architecture research for: Mobile Medical ACLS Application*
*Researched: 24/03/2026*