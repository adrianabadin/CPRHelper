# Phase 4: Data Persistence - Context

**Gathered:** 29/03/2026
**Status:** Ready for planning

<domain>
## Phase Boundary

Persistir sesiones de reanimación en base de datos local SQLite. El usuario puede guardar una sesión completa al finalizar un código, y luego buscar/visualizar sesiones pasadas. No se agrega exportación (PDF/CSV es Phase 5). No se agrega sincronización cloud.

Los 4 issues:
1. Almacenar sesiones en SQLite local (offline-first)
2. Capturar datos del paciente al guardar (nombre, apellido, DNI)
3. Permitir búsqueda de sesiones por datos del paciente y por fecha
4. Visualizar sesiones pasadas en HistorialPage

</domain>

<decisions>
## Implementation Decisions

### 1. Datos del paciente al guardar
- Al presionar FINALIZAR CÓDIGO, se muestra un popup/dialog pidiendo datos del paciente antes de guardar en BD
- **Campos obligatorios**: nombre, apellido, DNI
- Si el usuario no completa los campos, usar valor por defecto "SIN NOMBRE" para nombre/apellido y "SIN DNI" para DNI (para no bloquear el flujo en emergencias)
- UUID único como primary key de cada sesión (ya mencionado en deferred de Phase 03)

### 2. Datos de sesión persistidos
- **Todos los eventos del historial** de ese código se guardan — la colección completa de `EventRecord` del `IEventLogService`
- Cada `EventRecord` se asocia a la sesión via `SessionId` (foreign key)
- Session metadata que se guarda:
  - `SessionId` (UUID)
  - `PatientName`, `PatientLastName`, `PatientDNI`
  - `SessionStartTime` (DateTime)
  - `SessionEndTime` (DateTime)
  - `CreatedAt` (timestamp de guardado)
- Los eventos ya contienen: Timestamp, ElapsedSinceStart, EventType, Description, Details — todo se persiste tal cual
- H's y T's state: se persiste el estado final (IsChecked/IsDismissed por cada item) como eventos en el log (ya se loguean como eventos individuales)
- Timer state: NO se persiste por separado — la información relevante está en los eventos (ciclos, medicación administrada, etc.)
- Rhythm changes: ya están en el event log como "Ritmo actual: {ritmo}" — se persisten como parte de los eventos

### 3. Búsqueda de sesiones pasadas
- **Búsqueda por datos del paciente**: nombre, apellido, DNI (texto libre, filtra por coincidencia parcial case-insensitive)
- **Búsqueda por fecha**: rango de fechas (fecha desde / fecha hasta)
- Se puede combinar ambos criterios o usar individualmente
- HistorialPage evoluciona para mostrar:
  - Vista de lista de sesiones pasadas (orden cronológico inverso, más reciente primero)
  - Barra de búsqueda con filtros por paciente y fecha
  - Al seleccionar una sesión → vista de detalle con todos los eventos de esa sesión

### 4. Sesiones inmutables
- Una vez guardada, una sesión NO es editable
- No se puede modificar nombre, apellido, DNI, ni eventos
- No se puede eliminar sesiones desde la UI (datos clínicos — mantener integridad)
- Si hubo error en datos del paciente, queda registrado tal cual (audit trail)

### Claude's Discretion
- Estructura exacta de tablas SQLite (Session table + EventRecord table)
- Librería SQLite a usar (sqlite-net-pcl vs Microsoft.Data.Sqlite)
- Patrón de acceso a datos (repository pattern, servicio directo, etc.)
- DI registration del servicio de base de datos
- UI exacta del dialog de datos del paciente al finalizar
- Navegación entre lista de sesiones y detalle en HistorialPage
- Implementación del buscador (query SQLite con LIKE, filtros de fecha)
- Migración/inicialización de la base de datos al primer inicio

</decisions>

<specifics>
## Specific Ideas

- "Nombre, apellido y DNI" — mínimo necesario para identificar al paciente en el contexto de un código de reanimación
- "Todos los eventos del historial" — el event log completo es el registro clínico del código, no se pierde nada
- "No editables" — los datos clínicos una vez registrados no se modifican (audit trail, responsabilidad médica)
- "SIN NOMBRE" como default — no bloquear el flujo de guardado si no hay datos del paciente en una emergencia
- "Buscar por paciente y fecha" — los dos criterios más útiles para encontrar un código pasado

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **EventRecord model** — Ya tiene Id (string), Timestamp (DateTime), ElapsedSinceStart (TimeSpan), EventType (string), Description (string), Details (string?). Mapea directo a tabla SQLite. Solo necesita agregar `SessionId` como foreign key.
- **IEventLogService / EventLogService** — Colección in-memory `ObservableCollection<EventRecord>`. Guardar = tomar `.Events` y persistir cada item. No modificar la interfaz — crear un `ISessionRepository` nuevo.
- **HistorialPage.xaml/.cs** — Ya tiene CollectionView mostrando eventos con DataTemplate (ElapsedSinceStart, EventType, Description). Reutilizar el DataTemplate para la vista de detalle de sesión pasada.
- **MauiProgram.cs** — Registry central. Agregar registro de nuevo servicio de base de datos y repository aquí.
- **MainViewModel.StopCode()** — Momento natural para trigger de guardado. Ya para timers y session. Agregar popup de datos del paciente + llamada a guardar.

### Established Patterns
- **MVVM con CommunityToolkit** — `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`. Nuevo `HistorialViewModel` sigue este patrón.
- **Singleton para servicios compartidos** — IEventLogService, ITimerService son singleton. Nuevo ISessionRepository también singleton (una conexión SQLite).
- **DI via constructor** — ViewModels reciben servicios por inyección. HistorialPage ya recibe IEventLogService.
- **DisplayAlert para popups** — Ya usado para popups de ritmo, check de pulso, etc. Usar para dialog de datos del paciente al finalizar.
- **CollectionView con DataTemplate** — Patrón ya usado en HistorialPage y HsAndTsChecklist. Reutilizar para lista de sesiones.

### Integration Points
- **MainViewModel.StopCode()** → Agregar: mostrar popup de datos paciente → llamar `ISessionRepository.SaveSessionAsync()` con los eventos + metadata
- **HistorialPage.xaml** → Evolucionar: agregar sección "Sesiones Guardadas" con lista de sesiones pasadas + barra de búsqueda
- **HistorialPage.xaml.cs** → Cambiar BindingContext a nuevo `HistorialViewModel` que maneja tanto eventos en vivo como sesiones pasadas
- **MauiProgram.cs** → Registrar `ISessionRepository` como singleton + nueva conexión SQLite
- **AclsTracker.csproj** → Agregar paquete SQLite (sqlite-net-pcl o Microsoft.Data.Sqlite)
- **EventRecord** → Agregar propiedad `SessionId` (string, nullable para compatibilidad con eventos en vivo)

</code_context>

<deferred>
## Deferred Ideas

- Edición de sesiones guardadas (datos del paciente) — decisión explícita: no editar
- Eliminación de sesiones — decisión explícita: no eliminar (datos clínicos)
- Exportación PDF/CSV — Phase 5
- Sincronización cloud — v2 (SYNC-01)
- Historial con paginación — v2 (HIST-01)
- Notas libre-texto por sesión — considerar para v2

</deferred>

---

*Phase: 04-data-persistance*
*Context gathered: 29/03/2026*
