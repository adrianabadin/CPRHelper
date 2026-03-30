using AclsTracker.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QContainer = QuestPDF.Infrastructure.IContainer;
using QColors = QuestPDF.Helpers.Colors;

namespace AclsTracker.Services.Export;

/// <summary>
/// QuestPDF-based PDF generation for ACLS clinical reports.
/// Produces a structured PDF with 6 clinical sections per AHA ACLS 2020 protocol.
/// </summary>
public class PdfExportService : IPdfExportService
{
    /// <inheritdoc/>
    public async Task<string> GeneratePdfAsync(Session session, List<EventRecord> events)
    {
        var fileName = $"ACLS_{session.PatientLastName}_{session.SessionStartTime:yyyyMMdd_HHmm}.pdf";
        var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);

        // QuestPDF generation is CPU-bound; offload to background thread
        var bytes = await Task.Run(() => GeneratePdfBytes(session, events));

        await File.WriteAllBytesAsync(filePath, bytes);

        return filePath;
    }

    private byte[] GeneratePdfBytes(Session session, List<EventRecord> events)
    {
        var duration = session.SessionEndTime - session.SessionStartTime;
        var durationText = $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        var rhythmEvents = events.Where(e => e.EventType == "RhythmChange").ToList();
        var medicationEvents = events.Where(e => e.EventType == "Medication").ToList();
        var hsTsEvents = events.Where(e => e.EventType == "HsTs").ToList();
        var cprCycleEvents = events.Where(e => e.EventType == "CprCycle").ToList();
        var defibrillationEvents = events.Where(e => e.EventType == "Defibrillation").ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(ts => ts.FontSize(10));

                page.Content().Column(column =>
                {
                    // Section 1 — Header
                    column.Item().Element(compose => ComposeHeader(compose, session, durationText));
                    column.Item().PaddingVertical(8);

                    // Section 2 — Rhythm Summary
                    column.Item().Element(compose => ComposeRhythmSection(compose, rhythmEvents));
                    column.Item().PaddingVertical(6);

                    // Section 3 — Medications
                    column.Item().Element(compose => ComposeMedicationsSection(compose, medicationEvents));
                    column.Item().PaddingVertical(6);

                    // Section 4 — Reversible Causes (H's and T's)
                    column.Item().Element(compose => ComposeHsTsSection(compose, hsTsEvents));
                    column.Item().PaddingVertical(6);

                    // Section 5 — Compressions and Defibrillation
                    column.Item().Element(compose => ComposeCprSection(compose, cprCycleEvents, defibrillationEvents));
                    column.Item().PaddingVertical(6);

                    // Section 6 — Footer
                    column.Item().Element(ComposeFooter);
                });
            });
        });

        return document.GeneratePdf();
    }

    #region Section Composers

    private void ComposeHeader(QContainer container, Session session, string durationText)
    {
        container.Column(column =>
        {
            column.Item().Text("Resumen de Resucitación ACLS")
                .FontSize(16).Bold().FontColor(QColors.Blue.Darken2);

            column.Item().PaddingVertical(4);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(info =>
                {
                    info.Item().Text($"Paciente: {session.PatientLastName}, {session.PatientName}");
                    info.Item().Text($"DNI: {session.PatientDNI}");
                    info.Item().Text($"Fecha/Hora: {session.SessionStartTime:dd/MM/yyyy HH:mm}");
                    info.Item().Text($"Duración: {durationText}");
                });
            });
        });
    }

    private void ComposeSectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().Text(title)
            .FontSize(12).Bold().FontColor(QColors.Blue.Darken2);
        column.Item().PaddingVertical(2);
        column.Item().LineHorizontal(1).LineColor(QColors.Grey.Lighten2);
    }

    private void ComposeRhythmSection(QContainer container, List<EventRecord> rhythmEvents)
    {
        container.Column(column =>
        {
            ComposeSectionTitle(column, "Resumen de Ritmo");

            if (rhythmEvents.Count == 0)
            {
                column.Item().PaddingVertical(4)
                    .Text("Sin cambios de ritmo registrados")
                    .FontSize(9).FontColor(QColors.Grey.Darken1);
                return;
            }

            column.Item().Element(compose => ComposeEventTable(compose, rhythmEvents, "Tiempo", "Ritmo"));
        });
    }

    private void ComposeMedicationsSection(QContainer container, List<EventRecord> medicationEvents)
    {
        container.Column(column =>
        {
            ComposeSectionTitle(column, "Medicamentos Administrados");

            if (medicationEvents.Count == 0)
            {
                column.Item().PaddingVertical(4)
                    .Text("Sin medicamentos registrados")
                    .FontSize(9).FontColor(QColors.Grey.Darken1);
                return;
            }

            column.Item().Element(compose => ComposeEventTable(compose, medicationEvents, "Medicación", "Tiempo"));
        });
    }

    private void ComposeHsTsSection(QContainer container, List<EventRecord> hsTsEvents)
    {
        container.Column(column =>
        {
            ComposeSectionTitle(column, "Causas Reversibles (H's y T's)");

            if (hsTsEvents.Count == 0)
            {
                column.Item().PaddingVertical(4)
                    .Text("Sin causas reversibles registradas")
                    .FontSize(9).FontColor(QColors.Grey.Darken1);
                return;
            }

            column.Item().Element(compose => ComposeEventTable(compose, hsTsEvents, "Causa", "Tiempo"));
        });
    }

    private void ComposeCprSection(QContainer container, List<EventRecord> cprCycleEvents, List<EventRecord> defibrillationEvents)
    {
        container.Column(column =>
        {
            ComposeSectionTitle(column, "Compresiones y Ciclos");

            if (cprCycleEvents.Count == 0 && defibrillationEvents.Count == 0)
            {
                column.Item().PaddingVertical(4)
                    .Text("Sin registro de compresiones/desfibrilaciones")
                    .FontSize(9).FontColor(QColors.Grey.Darken1);
                return;
            }

            // Combine CPR cycles and defibrillations, sorted by time
            var combinedEvents = cprCycleEvents
                .Concat(defibrillationEvents)
                .OrderBy(e => e.ElapsedSinceStart)
                .ToList();

            column.Item().Element(compose => ComposeEventTable(compose, combinedEvents, "Evento", "Tiempo"));
        });
    }

    private void ComposeFooter(QContainer container)
    {
        container.AlignCenter().Text("Documento generado por ACLS Tracker — Protocolo AHA 2020")
            .FontSize(8).FontColor(QColors.Grey.Medium);
    }

    #endregion

    #region Table Helpers

    private void ComposeEventTable(QContainer container, List<EventRecord> events, string col1Title, string col2Title)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3f);
                columns.RelativeColumn(1f);
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text(col1Title).Bold();
                header.Cell().Element(CellStyle).Text(col2Title).Bold();
            });

            // Data rows
            foreach (var evt in events)
            {
                table.Cell().Element(CellStyle).Text(evt.Description);
                table.Cell().Element(CellStyle).Text(FormatElapsed(evt.ElapsedSinceStart));
            }
        });
    }

    private static QContainer CellStyle(QContainer container)
    {
        return container.BorderBottom(1).BorderColor(QColors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(4);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        return $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
    }

    #endregion
}
