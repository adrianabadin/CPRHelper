# Phase 03: Protocol Guidance - Context

**Gathered:** 29/03/2026
**Status:** Ready for planning

<domain>
## Phase Boundary

Generar recordatorios contextuales basados en el protocolo AHA ACLS 2020 durante la reanimación. El sistema muestra popups de recomendación según ritmo activo y situación temporal, el usuario acepta o rechaza cada recomendación, y la decisión se registra en el event log. Sin audio en esta fase (solo visual).

NO se agregan: audio de recordatorios, persistencia en base de datos, exportación de datos.

</domain>

<decisions>
## Implementation Decisions

### Sin audio
- NO se agregan recordatorios de audio en esta fase
- Solo popups visuales (DisplayAlert)
- Audio quedó explícitamente diferido a v2 (AUDI-04, TIME-04)

### Recordatorios en popup de check de pulso (cada 2 min)
Se agregan al popup de check de pulso existente (OnPulseCheckDue). El popup ya tiene texto de medicación. Los nuevos recordatorios se agregan como líneas adicionales en el mensaje ANTES de las sugerencias de medicación.

| Condición | Texto del recordatorio | Frecuencia |
|-----------|----------------------|------------|
| Primer ciclo (ciclo 0→1) | "¿Colocó acceso IV/IO?" | Solo primera vez |
| Siempre | "¿Rotar compresor?" | Cada check de pulso |
| H's y T's sin revisar | "Revisar H's y T's pendientes: {lista de nombres}" | Cada check si quedan pendientes |
| Timer Adrenalina >4min | "¿Hora de Adrenalina?" | Ya existe (02.1.1) |
| Timer Amiodarona >4min + TV/FV + dosis<2 | "¿Hora de Amiodarona? (Xra dosis: Xmg)" | Ya existe (02.1.1) |

La lista de H's y T's pendientes se obtiene de `EventRecordingViewModel.HsAndTsItems` donde `IsChecked == false && IsDismissed == false`.

### Recordatorios al cambiar de ritmo
Se muestran como popups separados (DisplayAlert) con botones ACEPTAR/RECHAZAR al momento del cambio de ritmo.

| Ritmo nuevo | Popup de recordatorio |
|------------|----------------------|
| **AESP** | "Buscar causas reversibles\nConsidere revisar H's y T's" |
| **Asistolia** | "Buscar causas reversibles\nConsidere revisar H's y T's" |
| **TV** | "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)" |
| **FV** | "Ritmo desfibrilable. Preparar desfibrilador.\nConsidere causas reversibles (H's y T's)" |
| **RCE** | "RCE alcanzado\n• Mantener vía aérea y ventilación\n• Monitorear ritmo y presión arterial\n• Obtener ECG 12 derivaciones\n• Considerar objetivo temp 32-36°C\n• Considerar causas reversibles" |

Nota: Los popups de TV/FV REEMPLAZAN el popup existente "Ritmo Desfibrilable - Defibrile y reanude compresiones" (líneas 39-44 de MainViewModel.cs). El nuevo popup incluye la sugerencia de desfibrilación + H's y T's.

### Interacción ACEPTAR/RECHAZAR
- Todos los recordatorios son popups DisplayAlert con dos botones: "ACEPTAR" y "RECHAZAR"
- **ACEPTAR** → registra en event log: "Recomendación aceptada: {texto resumido del recordatorio}"
- **RECHAZAR** → registra en event log: "Recomendación rechazada: {texto resumido del recordatorio}"
- Ambos cierran el popup y continúan el flujo normalmente
- No hay consecuencia funcional al aceptar o rechazar — es registro de decisión

### Tracking de ciclo para "primer ciclo"
- Se necesita un contador de ciclos (cuántas veces se disparó OnPulseCheckDue o se presionó NUEVO CICLO)
- El recordatorio de IV/IO se muestra solo en el primer check de pulso
- El contador se resetea en StartCode() y se incrementa en NewCycle() o OnPulseCheckDue()

### Defibrilación en event log
- Ya funciona correctamente — "Defibrilación realizada" se registra tanto desde botón DEFIBRILAR como desde popup de check de pulso. No requiere cambios.

</decisions>

<specifics>
## Specific Ideas

- "Rotar compresor en cada check de pulso" — previene fatiga del compresor, recomendación AHA estándar
- "IV/IO solo primer ciclo" — momento crítico para acceso vascular, innecesario repetir
- "H's y T's pendientes con lista de nombres" — recordar QUÉ causas quedan, no solo "hay pendientes"
- "RCE con cuidados post-paro completos" — checklist de estabilización post-RCE según AHA
- "TV/FV incluir H's y T's" — no solo desfibrilar, también pensar en causas reversibles
- "Sin audio" — mantener simplicidad, audio en futura fase

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- **MainViewModel.cs** — coordina todo. OnPulseCheckDue (líneas 119-169) ya construye mensaje dinámico con sugerencias de medicación. Agregar nuevas líneas de sugerencia al mismo flujo.
- **EventRecordingViewModel.cs** — expone `CurrentRhythm` (CardiacRhythm enum), `HsAndTsItems` (ObservableCollection), `LogCustomEventCommand`.
- **HsAndTsItem** — tiene `IsChecked`, `IsDismissed`, `Name`. Filtrar por `!IsChecked && !IsDismissed` para obtener pendientes.
- **TimerModel.cs** — `IsOverThreshold` ya funciona para todos los timer types.
- **AudioService + Plugin.Maui.Audio** — disponible pero NO se usa en esta fase.
- **CommunityToolkit.Maui.Alerts.Toast** — toast no bloqueante, ya usado para "Prepare desfibrilador".

### Established Patterns
- **DisplayAlert con dos botones** — `await DisplayAlert(title, message, "ACEPTAR", "RECHAZAR")` retorna bool (true = primer botón). Ya usado en popup de check de pulso con "DEFIBRILAR"/"CONTINUAR".
- **LogCustomEventCommand.Execute(string)** — registra evento con timestamp automático. Usar para registrar decisión de aceptar/rechazar.
- **PropertyChanged en EventRecordingViewModel** — MainViewModel ya escucha `CurrentRhythm` changes (líneas 33-46). Agregar lógica de popups de ritmo ahí.
- **IDispatcherTimer** — patrón de timers ya establecido (_pulseCheckTimer, _chargingWarningTimer).

### Integration Points
- **MainViewModel.OnPulseCheckDue** (líneas 119-169): Agregar líneas de sugerencia ANTES de las de medicación. El `var suggestions = new List<string>()` ya existe — agregar nuevas sugerencias ahí.
- **MainViewModel constructor** (líneas 33-46): El handler de `PropertyChanged` para `CurrentRhythm` ya muestra popup para TV/FV. Reemplazar con popup mejorado + agregar AESP, Asistolia, RCE.
- **MainViewModel.StartCode** (línea 52): Agregar reset de `_cycleCount = 0`.
- **MainViewModel.NewCycle** (línea 88): Agregar `_cycleCount++`.
- **EventRecordingViewModel.HsAndTsItems**: Acceder desde MainViewModel para filtrar pendientes.

</code_context>

<deferred>
## Deferred Ideas

- **Audio de recordatorios** — v2 requirements AUDI-04, TIME-04. Sintetización de voz o tonos cortos para recordatorios contextuales.
- **Grabar sesión en base de datos local** — NUEVA FASE PROPUESTA: "Persistencia de Sesiones". Incluye:
  - Guardar historial completo del paro en SQLite local
  - Botón "Grabar" en tab Historial
  - Datos del paciente: nombre, apellido, DNI
  - UUID único como primary key
  - Soporte para paciente "SIN NOMBRE"
  - Para planificar: `/gsd-discuss-phase` sobre esta nueva fase y luego insertar en roadmap

</deferred>

---

*Phase: 03-protocol-guidance*
*Context gathered: 29/03/2026*
