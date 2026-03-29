# Phase 4: Data Persistence - Research

**Researched:** 29/03/2026
**Domain:** SQLite local database for .NET MAUI mobile app
**Confidence:** HIGH

## Summary

This phase adds SQLite-based local persistence for completed resuscitation sessions. The app currently stores all event data in-memory via `ObservableCollection<EventRecord>` in `EventLogService`. On `StopCode()`, this data is lost. Phase 4 introduces a local SQLite database, a patient data capture dialog at save time, and a redesigned HistorialPage for browsing/searching past sessions.

**Primary recommendation:** Use `sqlite-net-pcl` (1.9.172) as the SQLite library. It provides simple attribute-based ORM mapping, built-in async support, and minimal boilerplate — perfectly suited to the project's MVVM + singleton service architecture. Create a new `ISessionRepository` service (singleton) that wraps `SQLiteAsyncConnection`, keeping `IEventLogService` unchanged for live session events.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
1. **Datos del paciente al guardar** — Al presionar FINALIZAR CÓDIGO, popup/dialog pidiendo nombre, apellido, DNI. Campos obligatorios con defaults "SIN NOMBRE"/"SIN DNI" si no se completan (no bloquear flujo en emergencias). UUID como PK.
2. **Datos de sesión persistidos** — Todos los eventos del historial se guardan completos. Cada EventRecord se asocia vía SessionId (FK). Metadata: SessionId (UUID), PatientName, PatientLastName, PatientDNI, SessionStartTime, SessionEndTime, CreatedAt. H's/T's y ritmo ya están como eventos en el log.
3. **Búsqueda de sesiones pasadas** — Texto libre (coincidencia parcial case-insensitive) por nombre/apellido/DNI + rango de fechas (desde/hasta). Combinables o individuales. HistorialPage evoluciona: lista cronológica inversa + barra de búsqueda + vista de detalle.
4. **Sesiones inmutables** — Una vez guardadas, NO editables, NO eliminables. Audit trail clínico.

### Claude's Discretion
- Estructura exacta de tablas SQLite (Session table + EventRecord table)
- Librería SQLite a usar (sqlite-net-pcl vs Microsoft.Data.Sqlite)
- Patrón de acceso a datos (repository pattern, servicio directo, etc.)
- DI registration del servicio de base de datos
- UI exacta del dialog de datos del paciente al finalizar
- Navegación entre lista de sesiones y detalle en HistorialPage
- Implementación del buscador (query SQLite con LIKE, filtros de fecha)
- Migración/inicialización de la base de datos al primer inicio

### Deferred Ideas (OUT OF SCOPE)
- Edición de sesiones guardadas (datos del paciente)
- Eliminación de sesiones
- Exportación PDF/CSV — Phase 5
- Sincronización cloud — v2 (SYNC-01)
- Historial con paginación — v2 (HIST-01)
- Notas libre-texto por sesión — considerar para v2
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| EXPO-01 | El usuario puede exportar datos de sesión en formato PDF | NOTE: This is Phase 5 per REQUIREMENTS.md. Phase 4 focuses on data persistence infrastructure that Phase 5 will build upon for export. |
| EXPO-02 | El usuario puede exportar datos de sesión en formato CSV | NOTE: This is Phase 5 per REQUIREMENTS.md. Phase 4 focuses on data persistence infrastructure that Phase 5 will build upon for export. |

**IMPORTANT NOTE on requirement IDs:** EXPO-01 and EXPO-02 map to Phase 5 (Exportación) per the traceability table in REQUIREMENTS.md. Phase 4 addresses the prerequisite data persistence layer — storing sessions so they CAN be exported later. The actual PDF/CSV export is deferred to Phase 5 per CONTEXT.md.
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| sqlite-net-pcl | 1.9.172 | Lightweight SQLite ORM for .NET | Simplest ORM for MAUI. Attribute-based table mapping fits existing model pattern. Built-in async via `SQLiteAsyncConnection`. 21.9M NuGet downloads, 4.4K GitHub stars. Uses SQLitePCLRaw.bundle_green for cross-platform native SQLite. |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| SQLitePCLRaw.bundle_green | 2.1.2+ | Native SQLite binaries per platform | Automatically pulled as dependency of sqlite-net-pcl. No separate install needed. |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| sqlite-net-pcl | Microsoft.Data.Sqlite | ADO.NET-style provider. More control but no ORM, no attribute mapping. Requires manual SQL for every operation. 3-5x more boilerplate for simple CRUD. Better for complex queries or when you need full SQL control. |
| sqlite-net-pcl | Entity Framework Core | Full ORM with migrations, LINQ, change tracking. Overkill for 2 tables. Heavy dependency for mobile. Slower cold start. Not recommended for MAUI with simple schemas. |

**Installation:**
```bash
dotnet add package sqlite-net-pcl --version 1.9.172
```

**Why NOT Microsoft.Data.Sqlite for this project:**
- No built-in ORM — would need manual CREATE TABLE, INSERT, SELECT for every operation
- The project has simple needs: 2 tables, basic CRUD, LIKE search
- sqlite-net-pcl maps C# classes to tables via attributes — matches existing `EventRecord` model
- Async API is built-in with `SQLiteAsyncConnection` — no manual connection management

## Architecture Patterns

### Recommended Project Structure
```
AclsTracker/
├── Models/
│   ├── EventRecord.cs        # EXISTING — add SessionId property
│   └── Session.cs            # NEW — database model for session metadata
├── Services/
│   ├── EventLog/             # EXISTING — unchanged, handles live session
│   └── Database/             # NEW — persistence layer
│       ├── ISessionRepository.cs
│       └── SessionRepository.cs
├── ViewModels/
│   └── HistorialViewModel.cs # NEW — manages both live events and saved sessions
└── Views/
    └── HistorialPage.xaml    # EVOLVED — tabbed/sectioned layout
```

### Pattern 1: Repository Pattern (Singleton Service)
**What:** A `SessionRepository` wraps `SQLiteAsyncConnection` and exposes high-level async methods. Registered as singleton in DI.
**When to use:** This is the only data access pattern needed for Phase 4.
**Example:**
```csharp
// Source: sqlite-net-pcl GitHub README + established project DI pattern
public interface ISessionRepository
{
    Task InitializeAsync();  // Create tables if not exist
    Task SaveSessionAsync(Session session, List<EventRecord> events);
    Task<List<Session>> SearchSessionsAsync(string? searchText, DateTime? fromDate, DateTime? toDate);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<List<EventRecord>> GetSessionEventsAsync(string sessionId);
}
```

### Pattern 2: Database Model with sqlite-net-pcl Attributes
**What:** Plain C# classes with `[Table]`, `[PrimaryKey]`, `[Indexed]` attributes for ORM mapping.
**When to use:** For all database entities.
**Example:**
```csharp
// Source: sqlite-net-pcl GitHub README
using SQLite;

namespace AclsTracker.Models;

[Table("Sessions")]
public class Session
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;           // UUID
    public string PatientName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientDNI { get; set; } = string.Empty;
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("EventRecords")]
public class EventRecordEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    [Indexed]
    public string SessionId { get; set; } = string.Empty;    // FK to Session
    public DateTime Timestamp { get; set; }
    public long ElapsedTicks { get; set; }                    // TimeSpan stored as ticks
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
}
```

### Pattern 3: Database Path Resolution (Cross-Platform)
**What:** Use `FileSystem.Current.AppDataDirectory` for the .db file location.
**When to use:** When creating the `SQLiteAsyncConnection`.
**Example:**
```csharp
// Source: Microsoft Learn — File system helpers
// https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers
var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "aclstracker.db3");
var database = new SQLiteAsyncConnection(dbPath);
```
**Platform locations:**
- **Android:** `Context.FilesDir` — backed up with Auto Backup (API 23+)
- **iOS/Mac Catalyst:** `Library/` directory — backed up by iTunes/iCloud
- **Windows:** `LocalFolder` directory — backed up to cloud

### Pattern 4: Async DB Initialization
**What:** Initialize database (create tables) lazily on first access, thread-safe.
**When to use:** In the repository constructor or via lazy init.
**Example:**
```csharp
public class SessionRepository : ISessionRepository
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;

    public SessionRepository()
    {
        var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "aclstracker.db3");
        _database = new SQLiteAsyncConnection(dbPath);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _database.CreateTableAsync<Session>().ConfigureAwait(false);
        await _database.CreateTableAsync<EventRecordEntity>().ConfigureAwait(false);
        _initialized = true;
    }
}
```

### Anti-Patterns to Avoid
- **Using EventRecord directly as DB entity:** The existing `EventRecord` inherits from `ObservableObject` (CommunityToolkit.Mvvm) which adds MVVM plumbing incompatible with sqlite-net-pcl serialization. Create a separate `EventRecordEntity` for DB storage and map between them.
- **Microsoft.Data.Sqlite for simple CRUD:** Way too much boilerplate. sqlite-net-pcl handles 2-table scenarios with zero SQL.
- **Storing TimeSpan as string:** Use `long` ticks (`TimeSpan.Ticks`) for lossless storage and easy reconstruction. String serialization risks parsing errors and locale issues.
- **Synchronous DB calls on UI thread:** Always use `SQLiteAsyncConnection` (not `SQLiteConnection`). MAUI apps must not block the UI thread with I/O.
- **Modifying IEventLogService:** Don't add persistence to the existing service. Create a new `ISessionRepository` that reads from `IEventLogService.Events` at save time.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| SQLite connection management | Manual connection strings, platform-specific paths | `FileSystem.Current.AppDataDirectory` + `SQLiteAsyncConnection` | Platform paths have GUID segments that change across builds (iOS). `FileSystem` handles this correctly. |
| Table creation | Raw `CREATE TABLE IF NOT EXISTS` SQL | `CreateTableAsync<T>()` | sqlite-net-pcl reads attributes, generates correct SQL, handles type mapping automatically. |
| CRUD operations | Manual INSERT/UPDATE/SELECT with parameters | `InsertAsync()`, `Table<T>().Where()`, `ToListAsync()` | Parameterized queries, type safety, no SQL injection risk. |
| Search (LIKE + date range) | String concatenation in SQL queries | Parameterized `QueryAsync<T>()` or LINQ-style `.Where()` | Prevents SQL injection. sqlite-net-pcl handles parameterization. |
| TimeSpan storage | Custom string format, double seconds, etc. | `TimeSpan.Ticks` (long) | Lossless roundtrip. No parsing errors, no precision loss, no locale issues. |

**Key insight:** sqlite-net-pcl is purpose-built for exactly this use case (simple mobile app with 1-5 tables). Every piece of manual SQLite code you'd write is already handled correctly by the library, including edge cases like thread safety, parameterized queries, and type mapping.

## Common Pitfalls

### Pitfall 1: ObservableObject in DB Models
**What goes wrong:** Trying to use `[ObservableProperty]` from CommunityToolkit.Mvvm on DB entity properties. sqlite-net-pcl uses reflection to read/write properties, and source-generated properties from `[ObservableProperty]` don't map correctly — the backing field `_elapsedTicks` is what gets reflected, not the public property.
**Why it happens:** Developers want one model for both UI binding and DB storage.
**How to avoid:** Use separate models: `EventRecord` (inherits `ObservableObject`, for UI) and `EventRecordEntity` (plain class with SQLite attributes, for DB). Map between them with simple conversion methods.
**Warning signs:** Data saves but reads back as default values (nulls, zeros).

### Pitfall 2: DB Initialization Race Condition
**What goes wrong:** Two concurrent calls to `EnsureInitializedAsync()` both try to `CreateTableAsync` simultaneously, potentially causing errors.
**Why it happens:** Singleton service, multiple ViewModels may trigger first access at the same time.
**How to avoid:** Use `SemaphoreSlim(1, 1)` to guard initialization:
```csharp
private readonly SemaphoreSlim _initLock = new(1, 1);

private async Task EnsureInitializedAsync()
{
    if (_initialized) return;
    await _initLock.WaitAsync().ConfigureAwait(false);
    try
    {
        if (_initialized) return; // double-check after acquiring lock
        await _database.CreateTableAsync<Session>().ConfigureAwait(false);
        await _database.CreateTableAsync<EventRecordEntity>().ConfigureAwait(false);
        _initialized = true;
    }
    finally
    {
        _initLock.Release();
    }
}
```
**Warning signs:** Intermittent `SQLiteException` on first app launch.

### Pitfall 3: iOS Path Changes on Reinstall
**What goes wrong:** Hardcoded absolute path to the .db file breaks after app reinstall or clean build on iOS simulator.
**Why it happens:** iOS app sandbox path contains a GUID that changes across installs.
**How to avoid:** Always construct the path at runtime using `FileSystem.Current.AppDataDirectory`. Never store or hardcode absolute paths.
**Warning signs:** "Database file not found" errors only on iOS after reinstall.

### Pitfall 4: Blocking UI During Save
**What goes wrong:** Using synchronous `SQLiteConnection` or calling `.Result` on async methods blocks the UI thread during save, freezing the app.
**Why it happens:** Save happens at `StopCode()` which is triggered from a `[RelayCommand]` (UI thread).
**How to avoid:** Make `StopCode` async, use `SQLiteAsyncConnection` exclusively, `await` all DB calls. The `[RelayCommand]` attribute in CommunityToolkit.Mvvm 8.x supports async methods natively.
**Warning signs:** UI freezes for 200ms+ when pressing FINALIZAR CÓDIGO.

### Pitfall 5: TimeSpan Precision Loss
**What goes wrong:** Storing `ElapsedSinceStart` as string or floating-point seconds loses sub-millisecond precision needed for REGI-02 (millisecond timestamps).
**Why it happens:** SQLite has no native TimeSpan type. Developers reach for `.ToString()` or `.TotalSeconds`.
**How to avoid:** Store as `long` ticks (`TimeSpan.Ticks`). Reconstruct with `TimeSpan.FromTicks(ticks)`. Ticks are 100-nanosecond intervals — full .NET DateTime precision preserved.
**Warning signs:** Event timestamps show rounding errors when displayed in detail view.

## Code Examples

Verified patterns from official sources:

### Save a Complete Session
```csharp
// Source: sqlite-net-pcl GitHub README async API section
public async Task SaveSessionAsync(Session session, List<EventRecord> liveEvents)
{
    await EnsureInitializedAsync().ConfigureAwait(false);

    // Start a transaction for atomic save
    await _database.RunInTransactionAsync(db =>
    {
        db.Insert(session);
        foreach (var evt in liveEvents)
        {
            var entity = new EventRecordEntity
            {
                Id = evt.Id,
                SessionId = session.Id,
                Timestamp = evt.Timestamp,
                ElapsedTicks = evt.ElapsedSinceStart.Ticks,
                EventType = evt.EventType,
                Description = evt.Description,
                Details = evt.Details
            };
            db.Insert(entity);
        }
    }).ConfigureAwait(false);
}
```

### Search Sessions with LIKE and Date Range
```csharp
// Source: sqlite-net-pcl GitHub README — QueryAsync method
public async Task<List<Session>> SearchSessionsAsync(
    string? searchText, DateTime? fromDate, DateTime? toDate)
{
    await EnsureInitializedAsync().ConfigureAwait(false);

    var sql = "SELECT * FROM Sessions WHERE 1=1";
    var args = new List<object>();

    if (!string.IsNullOrWhiteSpace(searchText))
    {
        sql += " AND (PatientName LIKE ? OR PatientLastName LIKE ? OR PatientDNI LIKE ?)";
        var pattern = $"%{searchText}%";
        args.Add(pattern);
        args.Add(pattern);
        args.Add(pattern);
    }

    if (fromDate.HasValue)
    {
        sql += " AND SessionStartTime >= ?";
        args.Add(fromDate.Value);
    }

    if (toDate.HasValue)
    {
        sql += " AND SessionStartTime <= ?";
        args.Add(toDate.Value.AddDays(1)); // inclusive of the end date
    }

    sql += " ORDER BY CreatedAt DESC";

    return await _database.QueryAsync<Session>(sql, args.ToArray()).ConfigureAwait(false);
}
```

### Get Session Events for Detail View
```csharp
// Source: sqlite-net-pcl GitHub README — Table<T>.Where pattern
public async Task<List<EventRecord>> GetSessionEventsAsync(string sessionId)
{
    await EnsureInitializedAsync().ConfigureAwait(false);

    var entities = await _database.Table<EventRecordEntity>()
        .Where(e => e.SessionId == sessionId)
        .OrderBy(e => e.Timestamp)
        .ToListAsync().ConfigureAwait(false);

    // Map back to UI model
    return entities.Select(e => new EventRecord
    {
        Id = e.Id,
        Timestamp = e.Timestamp,
        ElapsedSinceStart = TimeSpan.FromTicks(e.ElapsedTicks),
        EventType = e.EventType,
        Description = e.Description,
        Details = e.Details
    }).ToList();
}
```

### DI Registration in MauiProgram.cs
```csharp
// Source: Established pattern from existing MauiProgram.cs
// Add to existing service registrations:
builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddTransient<HistorialViewModel>();
```

### Mapping Between EventRecord (UI) and EventRecordEntity (DB)
```csharp
// Conversion methods — keep concerns separated
public static class EventRecordMapper
{
    public static EventRecordEntity ToEntity(this EventRecord record, string sessionId) => new()
    {
        Id = record.Id,
        SessionId = sessionId,
        Timestamp = record.Timestamp,
        ElapsedTicks = record.ElapsedSinceStart.Ticks,
        EventType = record.EventType,
        Description = record.Description,
        Details = record.Details
    };

    public static EventRecord ToModel(this EventRecordEntity entity) => new()
    {
        Id = entity.Id,
        Timestamp = entity.Timestamp,
        ElapsedSinceStart = TimeSpan.FromTicks(entity.ElapsedTicks),
        EventType = entity.EventType,
        Description = entity.Description,
        Details = entity.Details
    };
}
```

### Patient Data Popup in StopCode
```csharp
// Source: Established pattern — DisplayAlert already used throughout MainViewModel
// Note: For data entry popup, use a custom popup/page since DisplayAlert only shows text

// Option A: Simple DisplayPromptAsync (built-in, single field)
var name = await Application.Current!.MainPage!
    .DisplayPromptAsync("Datos del Paciente", "Nombre del paciente:", "OK", "Omitir");

// Option B: Custom popup page (recommended for 3 fields)
// Navigate to a modal page with Entry fields for nombre, apellido, DNI
// This is the recommended approach since we need 3 fields simultaneously
```

**Recommendation for patient dialog:** Use a simple modal `ContentPage` (not DisplayAlert) since we need 3 entry fields. The page should have:
- Entry for Nombre (placeholder "SIN NOMBRE" as hint)
- Entry for Apellido (placeholder "SIN NOMBRE" as hint)  
- Entry for DNI (placeholder "SIN DNI" as hint, Keyboard=Numeric)
- "GUARDAR" button (primary action)
- "OMITIR" button (saves with defaults, for emergencies)

Alternatively, use `CommunityToolkit.Maui` popup (`CommunityToolkit.Maui.Views.Popup`) which is already installed in the project. This provides a proper modal overlay without full-page navigation.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Environment.GetFolderPath` for DB path | `FileSystem.Current.AppDataDirectory` | MAUI migration | Handles iOS GUID-segment path changes automatically |
| Xamarin.Forms local database tutorial | MAUI Community Toolkit + sqlite-net-pcl | 2022+ | Same library, updated for MAUI DI patterns |
| Synchronous SQLite access | Async-only with `SQLiteAsyncConnection` | sqlite-net-pcl 1.5+ | Required for MAUI to avoid UI freezes |
| `sqlite-net-pcl` 1.8 | `sqlite-net-pcl` 1.9.172 | Mar 2024 | .NET 8 support, latest stable |

**Deprecated/outdated:**
- `SQLitePCL.raw` direct usage: superseded by `sqlite-net-pcl` which bundles it correctly
- `SQLiteConnection` (synchronous): still available but avoid in MAUI — use `SQLiteAsyncConnection`

## Open Questions

1. **Patient Dialog Implementation — CommunityToolkit Popup vs Modal Page**
   - What we know: `CommunityToolkit.Maui` v9.0.0 is already installed, which includes popup support. DisplayAlert only shows text — can't do data entry.
   - What's unclear: Whether to use `CommunityToolkit.Maui.Views.Popup` or a full modal ContentPage.
   - Recommendation: Use `CommunityToolkit.Maui.Views.Popup` — it's already in the project's dependencies, provides a proper modal overlay with binding support, and doesn't require Shell navigation changes. Lighter weight than a full page.

2. **HistorialPage Navigation — Tabs vs In-Page Sections**
   - What we know: HistorialPage is a Shell tab. It currently shows live session events only.
   - What's unclear: How to structure the dual-purpose view (live events + saved sessions).
   - Recommendation: Add a visual toggle at the top of HistorialPage — "Sesión Actual" / "Sesiones Guardadas". When viewing saved sessions, show search bar + session list. Tapping a session navigates to a detail view (could be the same page with state change, or a new `SessionDetailPage` pushed via Shell navigation).

3. **EXPO-01/EXPO-02 Requirement Mapping**
   - What we know: REQUIREMENTS.md maps EXPO-01/EXPO-02 to Phase 5 (Exportación). CONTEXT.md explicitly defers export to Phase 5.
   - What's unclear: Why the prompt specifies these IDs for Phase 4.
   - Recommendation: Phase 4 focuses on persistence infrastructure. The repository pattern and data models built here directly enable EXPO-01/EXPO-02 in Phase 5.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — Wave 0 setup required |
| Config file | None |
| Quick run command | N/A |
| Full suite command | N/A |

**Note:** No test infrastructure exists in the project. MAUI projects typically use xUnit or NUnit with a separate test project. For this phase, manual testing on Android emulator is the primary validation method given the MAUI-specific dependencies (FileSystem, SQLite platform libraries).

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| EXPO-01 (prerequisite) | Session saved to SQLite with all events | manual | N/A — MAUI runtime required | ❌ Wave 0 |
| EXPO-01 (prerequisite) | Patient data captured on save | manual | N/A | ❌ Wave 0 |
| EXPO-01 (prerequisite) | Sessions searchable by name/date | manual | N/A | ❌ Wave 0 |
| EXPO-01 (prerequisite) | Session detail shows all events | manual | N/A | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** Build + manual smoke test on Android emulator
- **Per wave merge:** Full manual test of save → search → detail flow
- **Phase gate:** Complete save/load/search cycle verified on at least one platform

### Wave 0 Gaps
- No automated test infrastructure exists. For a MAUI project, the most practical approach is:
  - Unit-testable logic (SessionRepository with in-memory SQLite): Could add `AclsTracker.Tests` project with xUnit
  - UI behavior: Manual verification on emulator
- [ ] `AclsTracker.Tests` project — for repository and mapping tests
- [ ] Test-only SQLite connection using `:memory:` database

## Sources

### Primary (HIGH confidence)
- sqlite-net-pcl GitHub README — API patterns, async examples, attribute mapping (https://github.com/praeclarum/sqlite-net)
- NuGet Gallery — sqlite-net-pcl 1.9.172 package details, compatibility info (https://www.nuget.org/packages/sqlite-net-pcl)
- Microsoft Learn — File system helpers for .NET MAUI, `FileSystem.Current.AppDataDirectory` (https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers)
- Project source code — EventRecord.cs, EventLogService.cs, MainViewModel.cs, MauiProgram.cs, HistorialPage.xaml

### Secondary (MEDIUM confidence)
- sqlite-net-pcl dependency chain: SQLitePCLRaw.bundle_green 2.1.2+ provides platform-native SQLite
- CommunityToolkit.Maui v9.0.0 — Popup component availability (already in project dependencies)
- Project CLAUDE.md — technology stack decisions (sqlite-net-pcl recommended in STACK.md)

### Tertiary (LOW confidence)
- None — all findings verified against official sources or project code

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — sqlite-net-pcl is the de facto standard for MAUI SQLite, verified via NuGet (21.9M downloads) and GitHub (4.4K stars). Library API verified from official README.
- Architecture: HIGH — Repository pattern + separate DB entities matches established project conventions (MVVM, singleton services, DI).
- Pitfalls: HIGH — TimeSpan/ticks, ObservableObject incompatibility, and iOS path issues are well-documented in the .NET MAUI community.
- Search implementation: HIGH — SQLite LIKE is case-insensitive by default for ASCII. Parameterized queries prevent injection. Date filtering is straightforward with SQLite datetime support.

**Research date:** 29/03/2026
**Valid until:** 30 days (stable libraries, no fast-moving dependencies)
