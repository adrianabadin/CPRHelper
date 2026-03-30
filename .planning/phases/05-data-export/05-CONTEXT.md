# Phase 5: Data Export - Context

**Gathered:** 30/03/2026
**Status:** Ready for planning

<domain>
## Phase Boundary

Exportar sesiones ACLS guardadas en formato PDF (reporte legible) y CSV (datos estructurados). La exportación se realiza desde el detail view de sesiones guardadas. Los archivos pueden compartirse via Share sheet o guardarse localmente.

Solo sesiones ya finalizadas y guardadas. Exportación durante código activo NO se incluye.

</domain>

<decisions>
## Implementation Decisions

### Formato PDF — Resumen de Resucitación Completo
- **Opción elegida:** A (Completo) — Datos paciente + metadata + línea de tiempo de ritmo + medicamentos + H's y T's + compresiones
- **Secciones del PDF:**
  1. **Header:** Datos del paciente (Nombre, Apellido, DNI) + fecha/hora del código + duración total
  2. **Resumen de ritmo:** Timeline visual de cambios de ritmo durante el código
  3. **Medicamentos:** Lista de medicamentos administrados con tiempos relativos
  4. **H's y T's:** Estado final de cada causa reversible (marcado/descartado)
  5. **Compresiones:** Indicación de compressiones realizadas (si hay registro)
  6. **Footer:** Nota de responsabilidad — "Documento generado por ACLS Tracker según protocolo AHA 2020"

### Formato CSV — Event Log Completo
- **Opción elegida:** A (Eventos completos)
- **Columnas:**
  - `Timestamp` — fecha y hora ISO del evento
  - `Tiempo_relativo` — tiempo transcurrido desde inicio del código (formato mm:ss)
  - `Tipo_evento` — tipo string del evento (RhythmChange, Medication, HsAndTs, Compression, etc.)
  - `Descripcion` — descripción legible del evento
  - `Detalles` — detalles adicionales si existen (dosis, ritmo, etc.)
  - `Ritmo_actual` — ritmo cardíaco al momento del evento
- **Encoding:** UTF-8 con BOM para compatibilidad con Excel en español
- **Delimiter:** coma (,)
- **Una fila por evento** — todos los eventos se exportan, no se resume

### Flujo de Exportación
- **Opción elegida:** A — Desde detail view de cada sesión guardada
- En el detail view de una sesión guardada, agregar dos botones:
  - "EXPORTAR PDF"
  - "EXPORTAR CSV"
- Los botones están en la vista de detalle de sesión individual
- Solo sesiones ya finalizadas — no se puede exportar durante código activo

### Destino del Archivo
- **Opción elegida:** A — Share sheet nativo + guardar localmente
- Al presionar exportar:
  1. Generar archivo (PDF o CSV)
  2. Abrir Share sheet del sistema (MAUI `Share.RequestAsync`)
  3. El usuario elige destino: email, WhatsApp, Files app, AirDrop, etc.
  4. Adicionalmente, guardar copia local en carpeta de descargas/documents del dispositivo

### Claude's Discretion
- Diseño visual exacto del PDF (fonts, tamaños, colores, logo/header)
- Librería PDF a usar (QuestPDF, iTextSharp, o PdfSharp)
- Nombres de archivo generados (formato sugerido: `ACLS_{PatientName}_{Date}.pdf`)
- Ubicación exacta de guardado local
- Implementación técnica del Share sheet y guardado local en cada plataforma

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- **Session model** — Ya tiene PatientName, PatientLastName, PatientDNI, SessionStartTime, SessionEndTime, CreatedAt
- **EventRecord model** — Ya tiene Timestamp, ElapsedSinceStart (TimeSpan), EventType, Description, Details, CurrentRhythm
- **ISessionRepository** — `GetSessionAsync(sessionId)` + `GetSessionEventsAsync(sessionId)` ya existen y funcionan
- **HistorialViewModel** — Detail view ya existe y muestra eventos de una sesión

### Established Patterns
- **CommunityToolkit.Maui** — Share.RequestAsync disponible para share sheet
- **MVVM con CommunityToolkit** — `[ObservableProperty]`, `[RelayCommand]`
- **Singleton servicios** — Repository patrón ya usado para SQLite

### Integration Points
- **SessionDetailPage** — Donde se agregan los botones PDF y CSV (o es otra página nueva?)
- **ISessionRepository** — Para obtener datos de la sesión a exportar
- **File system** — Para guardar copia local (FileSystem.Current.AppDataDirectory o Downloads)
- **MauiProgram.cs** — Registrar servicios de exportación si se crean nuevos

</code_context>

<specifics>
## Specific Ideas

- "PDF tipo resumen clínico" — secciones diferenciadas, header con datos paciente, fácil de leer en contexto hospitalario
- "CSV para análisis de datos" — event log completo, todos los eventos sin resumir, columnas fijas
- "Share sheet + guardar local" — máxima flexibilidad, usuario elige qué hacer con el archivo
- "Encoding UTF-8 con BOM" — para que Excel en español lea correctamente acentos y caracteres especiales

</specifics>

<deferred>
## Deferred Ideas

- Exportación durante código activo (mientras el código está corriendo) — no en scope de esta fase
- Exportación masiva de múltiples sesiones a la vez — una sesión a la vez por ahora
- Notas libre-texto por sesión antes de exportar —可以考虑 para v2

</deferred>

---

*Phase: 05-data-export*
*Context gathered: 30/03/2026*
