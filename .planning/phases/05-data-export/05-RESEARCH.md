# Phase 5: Data Export - Research

**Researched:** 30/03/2026
**Domain:** PDF generation (QuestPDF), CSV export, MAUI Share API, file system
**Confidence:** HIGH

## Summary

Phase 5 adds PDF and CSV export functionality to the existing session detail view in HistorialPage. The project already has a complete data layer (`ISessionRepository`, `Session`, `EventRecord` models) and a session detail view (`HistorialViewModel` with `IsDetailView` + `SelectedSession` + `SessionEvents`). The export services will be new classes that consume this existing data.

**QuestPDF 2026.2.4** is the recommended PDF library — it has a fluent C# API, MIT/community license (free for < $1M revenue), generates to byte[] or stream, supports tables/columns/sections natively, and works on .NET 9 MAUI. For CSV, given the simple flat structure (6 columns, one row per event), a hand-rolled StreamWriter with UTF-8 BOM is cleaner than pulling in CsvHelper for such trivial usage. MAUI's built-in `Share.RequestAsync` handles the share sheet natively on both Android and iOS.

**Primary recommendation:** Use QuestPDF for PDF generation, hand-roll CSV with StreamWriter + UTF-8 BOM, and leverage MAUI's built-in `Share.RequestAsync` API. Add export buttons to the existing session detail view in HistorialPage.xaml.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **PDF sections:** Header (patient data + date + duration) → Rhythm timeline → Medications list → H's and T's status → Compressions indicator → Footer (AHA 2020 disclaimer)
- **CSV columns:** Timestamp, Tiempo_relativo, Tipo_evento, Descripcion, Detalles, Ritmo_actual — UTF-8 with BOM, comma delimiter, one row per event, all events
- **Export trigger:** Two buttons ("EXPORTAR PDF", "EXPORTAR CSV") in the existing session detail view (IsDetailView)
- **File destination:** Share sheet native (MAUI `Share.RequestAsync`) + save local copy
- **Only finished sessions** — no export during active code

### Claude's Discretion
- Visual design of PDF (fonts, sizes, colors, layout)
- Library choice for PDF (QuestPDF recommended over iTextSharp)
- File naming convention (suggested: `ACLS_{PatientName}_{Date}.pdf`)
- Exact local save location
- Technical implementation details for share + local save

### Deferred Ideas (OUT OF SCOPE)
- Export during active code (only finished sessions)
- Bulk export of multiple sessions at once
- Free-text notes per session before export
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| EXPO-01 | El usuario puede exportar datos de sesión en formato PDF (reporte legible) | QuestPDF 2026.2.4 fluent API → generates byte[] → save to cache + Share sheet |
| EXPO-02 | El usuario puede exportar datos de sesión en formato CSV (datos estructurados) | StreamWriter with UTF-8 BOM encoding → comma-delimited 6-column output → save + Share sheet |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| QuestPDF | 2026.2.4 | PDF generation | Best-in-class .NET PDF library. Fluent C# API. MIT/community license. Generates to byte[] or stream. Works on .NET 6+ including .NET 9 MAUI. 19M+ NuGet downloads. |

### Built-in (No install needed)
| API | Purpose | Why |
|-----|---------|-----|
| `StreamWriter` + UTF-8 BOM | CSV generation | Trivial 6-column flat structure — no library needed. `new StreamWriter(stream, new UTF8Encoding(true))` handles BOM. |
| `Share.RequestAsync` | Native share sheet | Built into MAUI `Microsoft.Maui.ApplicationModel.DataTransfer`. Works on Android, iOS, Windows. |
| `FileSystem.Current.AppDataDirectory` | Local file storage | MAUI built-in. Returns app-private directory on all platforms. |
| `FileSystem.Current.CacheDirectory` | Temp files for sharing | MAUI built-in. Used for temp files before sharing. |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| QuestPDF | iTextSharp / iText7 | iText has AGPL license concerns and more complex API. QuestPDF has MIT/community license and simpler fluent API. |
| QuestPDF | PdfSharp | PdfSharp is outdated, less maintained, no fluent API. |
| Hand-rolled CSV | CsvHelper | CsvHelper (33M+ downloads) is excellent but overkill for 6 fixed columns. Avoids a dependency for trivial use. CsvHelper makes sense if CSV complexity grows in v2. |

**Installation:**
```bash
dotnet add package QuestPDF
```

Only QuestPDF needs to be added to `AclsTracker.csproj`. CSV and Share APIs are built into MAUI / BCL.

## Architecture Patterns

### Recommended Service Structure
```
AclsTracker/
├── Services/
│   └── Export/
│       ├── IPdfExportService.cs        # Interface for PDF generation
│       ├── PdfExportService.cs         # QuestPDF implementation
│       ├── ICsvExportService.cs        # Interface for CSV generation
│       ├── CsvExportService.cs         # StreamWriter implementation
│       └── ExportModels.cs            # PdfSessionReport model if needed
├── ViewModels/
│   └── HistorialViewModel.cs          # Add ExportPdfCommand, ExportCsvCommand
└── Views/
    └── HistorialPage.xaml             # Add export buttons to detail view
```

### Pattern 1: Export Service Interface
**What:** Thin service interfaces for PDF and CSV generation, injected into ViewModel
**When to use:** Always — separates data transformation from UI logic
**Example:**
```csharp
// Source: Established project pattern (MVVM + DI from MauiProgram.cs)
public interface IPdfExportService
{
    Task<string> GeneratePdfAsync(Session session, List<EventRecord> events);
    // Returns full file path of generated PDF
}

public interface ICsvExportService
{
    Task<string> GenerateCsvAsync(Session session, List<EventRecord> events);
    // Returns full file path of generated CSV
}
```

### Pattern 2: QuestPDF Document with Sections
**What:** QuestPDF fluent API with Page/Column/Table for structured clinical report
**When to use:** PDF generation for the ACLS session report
**Example:**
```csharp
// Source: QuestPDF official docs - quick-start.html, table/basics.html
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// Required license configuration (call once at app startup)
QuestPDF.Settings.License = LicenseType.Community;

public byte[] GeneratePdf(Session session, List<EventRecord> events)
{
    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10));

            // Header section
            page.Header().Column(col =>
            {
                col.Item().Text("Resumen de Resucitación ACLS")
                    .Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                col.Item().Text($"{session.PatientLastName}, {session.PatientName}");
                col.Item().Text($"DNI: {session.PatientDNI}");
                col.Item().Text($"Fecha: {session.SessionStartTime:dd/MM/yyyy HH:mm}");
            });

            // Content sections
            page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
            {
                col.Spacing(10);

                // Section: Medications
                col.Item().Text("Medicamentos Administrados").Bold().FontSize(12);
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Medicación");
                        header.Cell().Text("Tiempo");
                    });
                    foreach (var evt in events.Where(e => e.EventType == "Medication"))
                    {
                        table.Cell().Text(evt.Description);
                        table.Cell().Text($"{evt.ElapsedSinceStart:mm\\:ss}");
                    }
                });
            });

            // Footer
            page.Footer().AlignCenter()
                .Text("Documento generado por ACLS Tracker — Protocolo AHA 2020")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }).GeneratePdf();  // Returns byte[]
}
```

### Pattern 3: CSV with UTF-8 BOM
**What:** Simple StreamWriter with BOM for Excel Spanish compatibility
**When to use:** CSV export with accented characters
**Example:**
```csharp
// Source: Standard .NET BCL pattern
public async Task<string> GenerateCsvAsync(Session session, List<EventRecord> events)
{
    var fileName = $"ACLS_{session.PatientLastName}_{session.SessionStartTime:yyyyMMdd_HHmm}.csv";
    var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

    await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
    await using var writer = new StreamWriter(stream, new UTF8Encoding(true)); // true = BOM

    // Header row
    await writer.WriteLineAsync("Timestamp,Tiempo_relativo,Tipo_evento,Descripcion,Detalles,Ritmo_actual");

    // Data rows
    foreach (var evt in events.OrderBy(e => e.Timestamp))
    {
        var line = string.Join(",",
            evt.Timestamp.ToString("o"),                    // ISO 8601
            $"{evt.ElapsedSinceStart:mm\\:ss}",            // mm:ss format
            EscapeCsvField(evt.EventType),
            EscapeCsvField(evt.Description),
            EscapeCsvField(evt.Details ?? ""),
            ""  // CurrentRhythm not tracked per-event — see Data Gaps
        );
        await writer.WriteLineAsync(line);
    }

    return filePath;
}

private static string EscapeCsvField(string field)
{
    if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        return $"\"{field.Replace("\"", "\"\"")}\"";
    return field;
}
```

### Pattern 4: MAUI Share + Local Save
**What:** Generate to cache → share sheet → also save to app data
**When to use:** Every export action
**Example:**
```csharp
// Source: MAUI built-in API
public async Task ShareFileAsync(string filePath, string title)
{
    // 1. Save local copy to app documents
    var localDir = FileSystem.Current.AppDataDirectory;
    var localPath = Path.Combine(localDir, "Exports", Path.GetFileName(filePath));
    Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
    File.Copy(filePath, localPath, overwrite: true);

    // 2. Open share sheet
    await Share.RequestAsync(new ShareFileRequest
    {
        Title = title,
        File = new ShareFile(filePath),
        PresentationSourceBounds = DeviceInfo.Platform == DevicePlatform.iOS
            ? new Rect(0, 0, 0, 0)  // Required for iPad popover
            : Rect.Zero
    });
}
```

### Anti-Patterns to Avoid
- **Generating PDF to file path directly on UI thread:** QuestPDF `GeneratePdf()` is CPU-bound — wrap in `Task.Run` or use `GeneratePdf(stream)` with async file write
- **Using `GeneratePdf("filepath")` overload with hardcoded paths:** Use `GeneratePdf()` to get byte[], then write where needed. More flexible for share + local save.
- **Skipping UTF-8 BOM in CSV:** Spanish characters (á, é, í, ó, ú, ñ, ü) will corrupt in Excel without BOM
- **Storing export files permanently in CacheDirectory:** OS may clear cache. Always copy to AppDataDirectory for permanent local copy.
- **Not escaping CSV fields:** Commas/quotes in Description or Details will break CSV parsing

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| PDF layout engine | Custom PDF byte generation | QuestPDF | PDF format is complex (cross-reference tables, font embedding, encoding). QuestPDF handles all internals. |
| Share functionality | Custom platform-specific intents | `Share.RequestAsync` | MAUI abstracts Android Intent / iOS UIActivityViewController. Tested across platforms. |
| File system paths | Hardcoded `/sdcard/` or `NSFileManager` | `FileSystem.Current.AppDataDirectory` | MAUI provides platform-abstracted paths. Hardcoded paths break across Android versions / iOS sandbox. |

**Key insight:** The only thing worth hand-rolling is the CSV writer (6 columns, trivial). Everything else has battle-tested solutions.

## Common Pitfalls

### Pitfall 1: QuestPDF License Not Configured
**What goes wrong:** Runtime exception "Please configure your license" or watermark on generated PDFs
**Why it happens:** QuestPDF requires explicit license configuration since version 2022.12+
**How to avoid:** Add `QuestPDF.Settings.License = LicenseType.Community;` in `MauiProgram.cs` before `builder.Build()`
**Warning signs:** QuestPDF throws `LicenseNotConfiguredException` at first use

### Pitfall 2: CSV Encoding Issues with Spanish Characters
**What goes wrong:** Excel shows garbled characters for á, é, í, ó, ú, ñ in patient names and descriptions
**Why it happens:** Excel defaults to system locale encoding. Without BOM marker, it doesn't recognize UTF-8.
**How to avoid:** Always use `new UTF8Encoding(true)` (encoderShouldEmitUTF8Identifier = true) when creating the StreamWriter
**Warning signs:** Test with patient name containing accents (e.g., "García", "Ñoño")

### Pitfall 3: Share Sheet Crash on iPad
**What goes wrong:** `Share.RequestAsync` throws `PlatformNotSupportedException` or crashes on iPad
**Why it happens:** iPad requires `PresentationSourceBounds` for the share popover; it cannot present from a point
**How to avoid:** Always set `PresentationSourceBounds` in `ShareFileRequest`, even as `Rect.Zero` on non-iPad
**Warning signs:** Share works on iPhone but crashes on iPad

### Pitfall 4: File Locked During Share
**What goes wrong:** Share sheet opens but file appears empty or corrupted
**Why it happens:** File stream not disposed/flushed before passing to Share API
**How to avoid:** Use `await using` for StreamWriter, ensure file is fully written before calling `Share.RequestAsync`
**Warning signs:** Share sheet shows 0-byte file

### Pitfall 5: CurrentRhythm Column Gap
**What goes wrong:** CSV has empty `Ritmo_actual` column for all rows
**Why it happens:** The `EventRecord` model does NOT have a `CurrentRhythm` property. Rhythm information is only captured as `EventType == "RhythmChange"` with the rhythm name in the `Description` field.
**How to avoid:** Two options: (1) Parse rhythm from RhythmChange events to reconstruct timeline, or (2) Accept empty column for non-rhythm events and populate only for RhythmChange rows. Recommend option 2 for simplicity — rhythm state at time of event can be reconstructed from event log if needed.
**Warning signs:** Planner should note this as a known data model gap

## Code Examples

### Complete QuestPDF License Setup (MauiProgram.cs)
```csharp
// Source: QuestPDF official docs - license/community.html
// Add BEFORE builder.Build() in CreateMauiApp()
QuestPDF.Settings.License = LicenseType.Community;
```

### Export Command in ViewModel (MVVM pattern)
```csharp
// Source: Established pattern from HistorialViewModel.cs
[ObservableProperty]
private bool _isExporting;

[RelayCommand]
private async Task ExportPdf()
{
    if (SelectedSession is null) return;
    IsExporting = true;
    try
    {
        var events = SessionEvents.ToList();
        var filePath = await _pdfExportService.GeneratePdfAsync(SelectedSession, events);
        await _shareFileService.ShareFileAsync(filePath, "Exportar PDF");
    }
    catch (Exception ex)
    {
        await Shell.Current.DisplayAlert("Error", $"No se pudo generar el PDF: {ex.Message}", "OK");
    }
    finally
    {
        IsExporting = false;
    }
}
```

### QuestPDF Sectioned Report Structure
```csharp
// Source: QuestPDF docs - table/basics.html, column.html
page.Content().Column(col =>
{
    col.Spacing(15);

    // Section 1: Rhythm Summary (timeline of rhythm changes)
    col.Item().Element(SectionTitle).Text("Resumen de Ritmo");
    var rhythmEvents = events.Where(e => e.EventType == "RhythmChange").ToList();
    if (rhythmEvents.Any())
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(); });
            foreach (var evt in rhythmEvents)
            {
                table.Cell().Text($"{evt.ElapsedSinceStart:mm\\:ss}");
                table.Cell().Text(evt.Description);
            }
        });
    }
    else
    {
        col.Item().Text("Sin cambios de ritmo registrados").FontSize(9).FontColor(Colors.Grey.Medium);
    }

    // Section 2: Medications
    // Section 3: H's and T's
    // Section 4: Compressions
    // ... similar pattern for each section
});

// Reusable section title helper
static IContainer SectionTitle(IContainer container)
    => container.BorderBottom(1).BorderColor(Colors.Blue.Lighten2)
        .PaddingBottom(5).Text("").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
```

### DI Registration (MauiProgram.cs addition)
```csharp
// Source: Established pattern from existing MauiProgram.cs
builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
builder.Services.AddSingleton<ICsvExportService, CsvExportService>();
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| iTextSharp (AGPL) | QuestPDF (MIT/Community) | 2022+ | No license concerns for medical apps. Simpler API. |
| Xamarin.Essentials.Share | MAUI `Share.RequestAsync` | .NET MAUI release | Same API, now built into MAUI framework. |
| CsvHelper for everything | Hand-rolled for simple cases | Standard practice | 6 columns doesn't justify a dependency. |

**Deprecated/outdated:**
- iTextSharp (System.Drawing): Legacy, AGPL license issues, not MAUI-compatible. Use QuestPDF instead.
- PdfSharp: Poorly maintained, no native .NET 9 support. Use QuestPDF instead.

## Open Questions

1. **CurrentRhythm data gap**
   - What we know: `EventRecord` has no `CurrentRhythm` property. CONTEXT.md specifies `Ritmo_actual` as a CSV column. Rhythm info exists only in `RhythmChange` event descriptions.
   - What's unclear: Whether to reconstruct rhythm state per-event or leave column empty for non-rhythm events.
   - Recommendation: For RhythmChange events, extract rhythm from Description. For all other events, leave Ritmo_actual empty. This is the simplest correct approach.

2. **H's and T's data in PDF**
   - What we know: `HsAndTsItem` model has `IsChecked`, `IsDismissed`, `CheckedAt` properties. These are tracked in the UI during a session.
   - What's unclear: Whether H's and T's state is persisted in `EventRecord` entities with `EventType == "HsTs"` or if there's a separate persistence mechanism.
   - Recommendation: Check if H's and T's changes appear as events in SessionEvents. If `EventType == "HsTs"` events exist, parse them. If not, the PDF section may need to show "no data available" for H's and T's.

3. **Compression tracking data**
   - What we know: CONTEXT.md mentions "Indicación de compressiones realizadas (si hay registro)" — conditional section.
   - What's unclear: Whether compression events are logged with a specific EventType.
   - Recommendation: Filter events for compression-related types (`EventType.Contains("Compression")` or `EventType == "CprCycle"`). If none exist, show "Sin registro de compresiones" or omit section entirely.

## Data Model Analysis

### Existing Models (confirmed by code review)
| Model | Properties | Used For |
|-------|-----------|----------|
| `Session` | Id, PatientName, PatientLastName, PatientDNI, SessionStartTime, SessionEndTime, CreatedAt | Session metadata |
| `EventRecord` | Id, Timestamp, ElapsedSinceStart (TimeSpan), EventType, Description, Details | Individual events |
| `EventRecordEntity` | Id, SessionId, Timestamp, ElapsedTicks, EventType, Description, Details | SQLite persistence |

### Known EventTypes (from EventLogService.cs)
| EventType | Description format | PDF Section |
|-----------|-------------------|-------------|
| `"Session"` | "Inicio de código" / "Fin de código" | Header (start/end time) |
| `"RhythmChange"` | Rhythm name (e.g., "FV", "Asistolia") | Rhythm Summary |
| `"Medication"` | Drug name + dose | Medications |
| `"HsTs"` | H or T item name | H's and T's |
| `"CprCycle"` | Cycle info | Compressions |
| `"Defibrillation"` | Shock info | Event timeline |
| `"PulseCheck"` | Pulse check result | Event timeline |

### Key Integration Point
The `HistorialViewModel` already has:
- `SelectedSession` (Session?) — bound to session detail header
- `SessionEvents` (ObservableCollection<EventRecord>) — all events for selected session
- `IsDetailView` (bool) — controls visibility of detail section
- `IsExporting` — new property to disable buttons during export

Export buttons go in **Section 3 (Session Detail)** of `HistorialPage.xaml`, after the session info header Frame (Grid.Row=1) and before or alongside the events CollectionView (Grid.Row=2).

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None detected — manual testing recommended for Phase 5 |
| Config file | None |
| Quick run command | N/A |
| Full suite command | N/A |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| EXPO-01 | PDF export generates valid PDF with all sections | Manual | N/A — visual PDF inspection | ❌ |
| EXPO-01 | PDF contains patient data, events, footer | Manual | N/A — content verification | ❌ |
| EXPO-01 | Share sheet opens with PDF file | Manual | N/A — platform test | ❌ |
| EXPO-02 | CSV export generates UTF-8 BOM file | Unit (can be) | Manual verification | ❌ |
| EXPO-02 | CSV has all 6 columns with correct data | Manual | N/A — spreadsheet verification | ❌ |
| EXPO-02 | Spanish characters display correctly in Excel | Manual | N/A — Excel test | ❌ |

### Sampling Rate
- **Per task commit:** Build + manual launch test
- **Per wave merge:** Full manual test on Windows (primary dev platform)
- **Phase gate:** Test both PDF and CSV export end-to-end on at least one platform

### Wave 0 Gaps
- No automated test infrastructure exists for this project
- Phase 5 is primarily UI + file generation — manual testing is appropriate
- Consider adding unit test for CSV escaping logic if time permits

## Sources

### Primary (HIGH confidence)
- NuGet QuestPDF 2026.2.4 — verified latest version, license (MIT Community), .NET 9 compatibility
- QuestPDF official docs (questpdf.com) — API patterns, output types (GeneratePdf byte[] overload), table basics, license configuration
- Existing codebase review — Session.cs, EventRecord.cs, EventRecordEntity.cs, ISessionRepository.cs, SessionRepository.cs, HistorialViewModel.cs, HistorialPage.xaml

### Secondary (MEDIUM confidence)
- MAUI `Share.RequestAsync` — known API from training data, consistent with project's CommunityToolkit.Maui usage
- MAUI `FileSystem.Current.AppDataDirectory` — standard MAUI Essentials API
- UTF-8 BOM for Excel compatibility — standard encoding practice

### Tertiary (LOW confidence)
- None — all findings verified through primary source code review or official docs

## Metadata

**Confidence breakdown:**
- Standard stack (QuestPDF): HIGH — verified via NuGet + official docs, latest version confirmed March 2026
- Architecture patterns: HIGH — derived from existing codebase patterns (MVVM, DI, services)
- CSV approach: HIGH — trivial BCL usage, no external dependency
- Share API: MEDIUM — MAUI built-in API, confirmed in project's CommunityToolkit.Maui dependency
- Pitfalls: HIGH — QuestPDF license config verified in official docs, UTF-8 BOM is standard practice
- Data model gaps: HIGH — confirmed by reading actual source code of all relevant models

**Research date:** 30/03/2026
**Valid until:** 30/04/2026 (QuestPDF is stable, MAUI APIs are mature)
