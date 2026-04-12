# Graph Report - AclsTracker  (2026-04-11)

## Corpus Check
- Large corpus: 2018 files · ~1,959,315 words. Semantic extraction will be expensive (many Claude tokens). Consider running on a subfolder, or use --no-semantic to run AST-only.

## Summary
- 406 nodes · 473 edges · 36 communities detected
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## God Nodes (most connected - your core abstractions)
1. `SessionSyncService` - 21 edges
2. `AuthViewModel` - 18 edges
3. `HistorialViewModel` - 18 edges
4. `TimerViewModel` - 18 edges
5. `MainViewModel` - 17 edges
6. `SessionRepository` - 16 edges
7. `TimerService` - 15 edges
8. `ISessionRepository` - 14 edges
9. `AuthService` - 11 edges
10. `EventRecordingViewModel` - 11 edges

## Surprising Connections (you probably didn't know these)
- `AuthViewModel` --inherits--> `ObservableObject`  [EXTRACTED]
  AclsTracker\ViewModels\AuthViewModel.cs →   _Bridges community 3 → community 7_
- `HistorialViewModel` --inherits--> `ObservableObject`  [EXTRACTED]
  AclsTracker\ViewModels\HistorialViewModel.cs →   _Bridges community 3 → community 8_
- `MainViewModel` --inherits--> `ObservableObject`  [EXTRACTED]
  AclsTracker\ViewModels\MainViewModel.cs →   _Bridges community 3 → community 11_
- `TimerViewModel` --inherits--> `ObservableObject`  [EXTRACTED]
  AclsTracker\ViewModels\TimerViewModel.cs →   _Bridges community 3 → community 9_
- `TimerService` --inherits--> `IDisposable`  [EXTRACTED]
  AclsTracker\Services\Timer\TimerService.cs →   _Bridges community 16 → community 5_

## Communities

### Community 0 - "Value Converters"
Cohesion: 0.06
Nodes (9): BoolToActiveColorConverter, BoolToActiveTextColorConverter, BoolToColorConverter, BoolToOnOffConverter, BoolToOpacityConverter, InvertBoolConverter, IsNotNullConverter, IValueConverter (+1 more)

### Community 1 - "Pages / Views"
Cohesion: 0.07
Nodes (9): ContentPage, HistorialPage, HsAndTsPage, LoginPage, MainPage, ProfilePage, PulseCheckPage, RegisterPage (+1 more)

### Community 2 - "Session Sync Service"
Cohesion: 0.13
Nodes (3): ISessionSyncService, ISessionSyncService, SessionSyncService

### Community 3 - "Event Recording ViewModel"
Cohesion: 0.08
Nodes (6): EventRecord, EventRecordingViewModel, HsAndTsItem, MetronomeViewModel, ObservableObject, TimerModel

### Community 4 - "Auth Service"
Cohesion: 0.09
Nodes (3): AuthService, IAuthService, Session

### Community 5 - "Timer Service"
Cohesion: 0.1
Nodes (3): ITimerService, ITimerService, TimerService

### Community 6 - "Custom UI Controls"
Cohesion: 0.1
Nodes (9): AuthAvatarControl, ContentView, EventLogPanel, HsAndTsChecklist, INotifyPropertyChanged, MetronomePulse, NotificationBanner, RhythmSelector (+1 more)

### Community 7 - "Auth ViewModel"
Cohesion: 0.12
Nodes (1): AuthViewModel

### Community 8 - "Historial ViewModel"
Cohesion: 0.15
Nodes (1): HistorialViewModel

### Community 9 - "Timer ViewModel"
Cohesion: 0.22
Nodes (1): TimerViewModel

### Community 10 - "PDF Export"
Cohesion: 0.23
Nodes (12): IPdfExportService, IPdfExportService, ComposeCprSection(), ComposeEventTable(), ComposeHeader(), ComposeHsTsSection(), ComposeMedicationsSection(), ComposeRhythmSection() (+4 more)

### Community 11 - "Main ACLS ViewModel"
Cohesion: 0.18
Nodes (1): MainViewModel

### Community 12 - "Session Repository (impl)"
Cohesion: 0.23
Nodes (1): SessionRepository

### Community 13 - "Session Repository (interface)"
Cohesion: 0.13
Nodes (2): ISessionRepository, ISessionRepository

### Community 14 - "Event Log Service"
Cohesion: 0.2
Nodes (2): EventLogService, IEventLogService

### Community 15 - "Metronome Service"
Cohesion: 0.2
Nodes (3): IMetronomeService, IMetronomeService, MetronomeService

### Community 16 - "Audio Service"
Cohesion: 0.22
Nodes (3): AudioService, IAudioService, IDisposable

### Community 17 - "Supabase Models"
Cohesion: 0.29
Nodes (4): BaseModel, EventSupabase, SessionSupabase, UserProfile

### Community 18 - "CSV Export"
Cohesion: 0.33
Nodes (2): CsvExportService, ICsvExportService

### Community 19 - "Windows App Entry"
Cohesion: 0.33
Nodes (3): App, Application, MauiWinUIApplication

### Community 20 - "Supabase Session Persistence"
Cohesion: 0.33
Nodes (2): IGotrueSessionPersistence, SupabaseSessionHandler

### Community 21 - "Patient Data Popup"
Cohesion: 0.4
Nodes (2): PatientDataPopup, Popup

### Community 22 - "Android App Class"
Cohesion: 0.5
Nodes (2): MainApplication, MauiApplication

### Community 23 - "iOS App Delegate"
Cohesion: 0.5
Nodes (2): AppDelegate, MauiUIApplicationDelegate

### Community 24 - "Event Record Mapper"
Cohesion: 0.5
Nodes (1): EventRecordMapper

### Community 25 - "App Shell"
Cohesion: 0.67
Nodes (2): AppShell, Shell

### Community 26 - "MAUI Program Bootstrap"
Cohesion: 0.67
Nodes (1): MauiProgram

### Community 27 - "Supabase Config"
Cohesion: 0.67
Nodes (1): SupabaseConfig

### Community 28 - "Android Main Activity"
Cohesion: 0.67
Nodes (2): MainActivity, MauiAppCompatActivity

### Community 29 - "Android Auth Callback"
Cohesion: 0.67
Nodes (2): WebAuthenticationCallbackActivity, WebAuthenticatorCallbackActivity

### Community 30 - "MacCatalyst Entry"
Cohesion: 0.67
Nodes (1): Program

### Community 31 - "Event Record Entity"
Cohesion: 1.0
Nodes (1): EventRecordEntity

### Community 32 - "Sync Queue Item"
Cohesion: 1.0
Nodes (1): SyncQueueItem

### Community 33 - "Cardiac Rhythm Enum"
Cohesion: 1.0
Nodes (0): 

### Community 34 - "Timer Type Enum"
Cohesion: 1.0
Nodes (0): 

### Community 35 - "Sync State Enum"
Cohesion: 1.0
Nodes (0): 

## Knowledge Gaps
- **3 isolated node(s):** `EventRecordEntity`, `Session`, `SyncQueueItem`
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Event Record Entity`** (2 nodes): `EventRecordEntity.cs`, `EventRecordEntity`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Sync Queue Item`** (2 nodes): `SyncQueueItem.cs`, `SyncQueueItem`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Cardiac Rhythm Enum`** (1 nodes): `CardiacRhythm.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Timer Type Enum`** (1 nodes): `TimerType.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Sync State Enum`** (1 nodes): `SyncState.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `AuthViewModel` connect `Auth ViewModel` to `Event Recording ViewModel`?**
  _High betweenness centrality (0.018) - this node is a cross-community bridge._
- **Why does `HistorialViewModel` connect `Historial ViewModel` to `Event Recording ViewModel`?**
  _High betweenness centrality (0.018) - this node is a cross-community bridge._
- **Why does `TimerViewModel` connect `Timer ViewModel` to `Event Recording ViewModel`?**
  _High betweenness centrality (0.018) - this node is a cross-community bridge._
- **What connects `EventRecordEntity`, `Session`, `SyncQueueItem` to the rest of the system?**
  _3 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Value Converters` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._
- **Should `Pages / Views` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._
- **Should `Session Sync Service` be split into smaller, more focused modules?**
  _Cohesion score 0.13 - nodes in this community are weakly interconnected._