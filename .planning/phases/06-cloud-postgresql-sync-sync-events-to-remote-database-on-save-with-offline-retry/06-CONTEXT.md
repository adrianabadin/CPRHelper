# Phase 6: Cloud PostgreSQL Sync - Context

**Gathered:** 31/03/2026
**Status:** Ready for planning
**Updated from:** Previous CONTEXT.md (30/03/2026), revised after Phase 05.2

<domain>
## Phase Boundary

Sincronización automática y continua entre dispositivos del mismo usuario. Cuando una sesión se guarda en un dispositivo, aparece automáticamente en todos los demás dispositivos del usuario. El usuario ve ses sesiones de otros dispositivos en su Historial sin intervención manual.
This phase EXTENDS the ISessionSyncService created in Phase 05.2, adding Supabase Realtime subscriptions for the background.
</domain>

<decisions>
## Implementation Decisions

### 1. Mecanismo de sync en tiempo real
- **Supcripción en tiempo real de Supabase (postgres_changes)** — Se suscribe a la tabla `sessions` en Supabase para cambios del usuario actual. Cuando se inserta una sesión desde cualquier dispositivo, todos los demás dispositivos con la app abierta recib la notificación en tiempo real
- **WebSocket nativo de Supabase** — el SDK de Supabase C# ya soporta Realtime a través de Postgrest. Se usa como transporte en lugar de HTTP polling
- **Monitoreo de conectividad como respaldo** — Si el WebSocket falla, se usa monitoreo de conectividad + sync periódico (ej: cada 30 minutos) como fallback
- **Cola de re memoria persistida** — las sesiones pendientes de sync de 05.2 (en memoria) se persisten en SQLite (tabla separada o campo en Session) para sobrevivir reinicios de la app
- **Procesamiento en segundo plano** — Subscription handler corre sync en un servicio en segundo plano con `Task.RunInBackground`

### 2. Comportamiento multi-dispositivo
- **Ambos dispositivos sincronizan automáticamente** — Si el usuario guarda en teléfono y tablet al mismo tiempo, ambas sesiones aparecen en ambos dispositivos eventualmente
- **No hay conflictos reales** — Las sesiones son inmutables con GUID único. Si llega la misma sesión de dos dispositivos, la segunda se ignora (GUID dedup)
- **GUID dedup** — Antes de insertar una sesión descargada, verificar si el `SessionId` ya existe en SQLite local (ya implementado en 05.2)
- **Merge al re-login** — Al re-login, el flujo de 05.2 (claim orphans + download all) se mantiene. Las nuevas sesiones de otros dispositivos se suman a las existentes

### 3. Notificaciones al usuario
- **Notificación sutil tipo toast** — "N sesiones sincronizadas" aparece brevement cuando se detectan sesiones nuevas de otros dispositivos. No es intrusivo, no bloquea al usuario
- **Sin notificación en sesiones creadas localmente** — Si el usuario guardó la sesión en este dispositivo, no hay notificación (ya la vio en 05.2). Solo se notifica cuando llega de otro dispositivo
- **Toast auto-dismiss** — La notificación desaparece después de 3-5 segundos automáticamente

### 4. Indicador de estado de sincronización
- **Expandir el ☁️ en 05.2** — El indicador actual se mantiene, pero se agrega un indicador general del estado de sync:
  - ☁️ (verde) — Sincronizado y en tiempo real
  - ☁️ (amarillo) — Sincronizando (procesando cola de pendientes o descargando)
  - ☁️ (gris) — Sin conexión, pendientes locales en cola
- **Posición** — Mismo lugar que el ☁️ de 05.2 (al lado del nombre del paciente en Historial)
- **Sin indicador global** — No hay barra de estado de sync en la app. Solo indicadores por sesión en el Historial

### Claude's Discretion
- Estructura exacta del handler de suscripción en tiempo real (callback, threading, error handling)
- Intervalo del fallback polling (si WebSocket no disponible)
- Estructura de la cola persistida en SQLite (campo en Session vs tabla separada)
- Formato exacto del toast de notificación
- Colores exactos del indicador ☁️ según estado (amarillo/gris/verde)
- Manejo de reconexión del WebSocket (reconnect automático vs manual)
- Threading del procesamiento (Task.RunInBackground vs hilo)

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets (from Phase 05.2)
- **ISessionSyncService** — Interface con UploadSessionAsync, ClaimOrphanSessionsAsync, DownloadUserSessionsAsync, DeleteLocalUserSessionsAsync. Se extiende con SubscribeToSessionChanges y procesamiento en segundo plano.
- **SessionSyncService** — Implementación existente que coordina upload/download/claim/cleanup. Se extiende para agregar subscription en tiempo real y background sync.
- **SessionSupabase / EventSupabase** — Modelos Supabase con [Table("sessions")] y [Table("events")]. Ya usados para insertar/query. Se usan para suscripción en tiempo real.
- **SessionRepository** — SQLite local con métodos extendidos (GetOrphanSessionsAsync, DeleteByUserIdAsync, InsertDownloadedSessionAsync). Se puede extender con persistencia de cola.
- **HistorialPage.xaml** — Ya tiene indicador ☁️ de 05.2. Se extiende para soportar estados de color (amarillo/gris/verde).
- **MauiProgram.cs** — Registro de servicios existente. Se puede agregar registro del listener de conectividad.

### Established Patterns
- **MVVM con CommunityToolkit** — [ObservableProperty], [RelayCommand], ObservableObject
- **Singleton para servicios** — ISessionSyncService es singleton. El subscription handler también singleton (parte del mismo servicio)
- **Supabase Postgrest Models** — [Table], [Column], BaseModel. Pattern para realtime: los mismos modelos se usan para subscription
- **DI via constructor** — Servicios se inyectan por constructor
- **AuthStateChanged event** — Patrones de eventos para re:e IAuthService. El nuevo subscription handler sigue un patrón similar.

### Integration Points
- **ISessionSyncService** → Se extiende con SubscribeToSessionChanges(userId) y StartBackgroundSync/StopBackgroundSync
- **MainViewModel.StopCode()** → Ya hookado para upload en 05.2. No se necesita cambios para realtime (el subscription ya lo detecta)
- **AuthViewModel.SignOutAsync()** → Ya hookado para cleanup en 05.2. Se agrega StopBackgroundSync antes del cleanup
- **HistorialViewModel** → Se extiende para escuchar notificaciones de nuevas sesiones sincronizadas y actualizar indicadores
- **HistorialPage.xaml** → Se extiende el DataTemplate para soportar colores dinámicos en el ☁️
- **App.xaml.cs** → Se puede agregar inicialización del background sync en OnStart o similar
- **MauiProgram.cs** → No se necesitan cambios (ISessionSyncService ya registrado)
- **Supabase Dashboard** → Habilitar Realtime para las tablas sessions y events (toggle en Dashboard)

</code_context>

<specifics>
## Specific Ideas

- "Sync en tiempo real" — cambios aparecen automáticamente en otros dispositivos sin intervención del usuario
- La app funciona offline — si no hay conexión, las sesiones se guardan localmente y se sincronizan cuando vuelve la conexión
- Notificación sutil tipo "2 sesiones sincronizadas" — informativa, no intrusiva
- ☁️ con colores dinámicos — verde (sincronizado), amarillo (sincronizando), gris (sin conexión)

</specifics>

<deferred>
## Deferred Ideas

- **Dashboard web para consultar sesiones en la nube** — nueva capability, sería su propia fase
- **Sync selectivo** (elegir qué sesiones sincronizar) — contradice el diseño de sync automático
- **Compartir sesiones con miembros del equipo** — requiere tabla de permisos y RLS más compleja. Sería su propia fase
- **Limitación de sesiones locales por storage** — sin límite por ahora

</deferred>

---

*Phase: 06-cloud-postgresql-sync*
*Context gathered: 31/03/2026 (updated from original 30/03/2026)*
