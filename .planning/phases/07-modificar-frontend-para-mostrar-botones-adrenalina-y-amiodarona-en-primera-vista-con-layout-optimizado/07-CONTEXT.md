# Phase 7: Modificar Frontend — Botones Adrenalina y Amiodarona en Primera Vista - Context

**Gathered:** 2026-04-04
**Status:** Ready for planning

<domain>
## Phase Boundary

Optimizar el layout de MainPage.xaml para que los botones ADRENALINA y AMIODARONA sean visibles sin scroll en pantallas estándar de Android. No se agregan funcionalidades nuevas — se reorganizan y comprimen los bloques existentes para ganar espacio vertical.

</domain>

<decisions>
## Implementation Decisions

### Reordenamiento de bloques
- INICIAR/FINALIZAR CODIGO se mantiene arriba (posición actual)
- El orden de bloques no cambia: INICIAR → Metrónomo → Ritmo → Timers → Acciones → EventLog
- El espacio se gana reduciendo alturas y comprimiendo, no reordenando
- Shell title bar y AuthAvatarControl se mantienen como están

### Selector de ritmo — una sola fila
- Los 5 botones de ritmo (RCE, AESP, ASISTOLIA, TV, FV) deben entrar en UNA sola fila horizontal
- Reducir font a 11-12px y HeightRequest a ~36px para que quepan sin wrap
- Reemplazar FlexLayout wrap por Grid o HorizontalStackLayout de fila única

### Agrupación de botones de acción — 2 filas en vez de 3
- Fila 1: NUEVO CICLO + DEFIBRILAR lado a lado (50/50 igual ancho)
- Fila 2: ADRENALINA + AMIODARONA lado a lado (como están)
- Eliminar DEFIBRILAR como botón de ancho completo — pasa a compartir fila con NUEVO CICLO
- HeightRequest de los 4 botones: 40px (reducido de 44px)
- Colores sin cambios: NUEVO CICLO naranja (#FF9800), DEFIBRILAR rojo (#C62828), drogas violeta (#7B1FA2)

### Reducción de alturas
- Metrónomo: círculo animado de 50px → 45px
- TimerCards: padding interno de 8px → 4px, font elapsed de 24px → 20px
- Grid de timers: RowSpacing/ColumnSpacing se mantienen o se reducen levemente (Claude's discretion)
- Botón INICIAR CODIGO: mantener 48px (es el botón más importante)
- Spacing y Padding del VerticalStackLayout: Claude decide valores óptimos

### Comportamiento pre/post código
- Antes de INICIAR CODIGO: todo visible pero deshabilitado (timers en 00:00, botones grises)
- INICIAR/FINALIZAR se alternan en la misma posición arriba (como está implementado)

### Claude's Discretion
- Valores exactos de Spacing y Padding del VerticalStackLayout (actualmente Spacing=8, Padding=12)
- RowSpacing/ColumnSpacing del grid de timers (actualmente 6px)
- Ajustes finos de font sizes en TimerCard labels

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `MainPage.xaml` (líneas 1-153): Layout actual con todos los bloques — editar in-place
- `MetronomePulse.xaml`: Control compacto horizontal — reducir HeightRequest del círculo
- `TimerCard.xaml`: Control de timer — reducir padding y font del elapsed time
- `RhythmSelector.xaml`: Control de 5 botones de ritmo — cambiar a fila única

### Established Patterns
- MVVM con CommunityToolkit (ObservableProperty, RelayCommand)
- IsVisible bindings para alternar INICIAR/FINALIZAR
- DataTrigger en botones de drogas para highlight sugerencia (IsAdrenalinaSuggested, IsAmiodaronaSuggested)
- BoolToOpacityConverter para AMIODARONA deshabilitada

### Integration Points
- `MainPage.xaml`: Reorganizar Grid de botones de acción (líneas 88-145)
- `RhythmSelector.xaml`: Cambiar layout de botones a fila única
- `MetronomePulse.xaml`: Reducir tamaño del círculo animado
- `TimerCard.xaml`: Reducir padding y font sizes

</code_context>

<specifics>
## Specific Ideas

- "Quiero reducir el espacio vacío de arriba de INICIAR CODIGO" — el espacio del Shell/TabBar contribuye
- "Disminuir la altura de los metrónomos un poco" — círculo de 50px a 45px
- "Achicar un poquito los botones de ritmo para que entren en una sola fila"
- Los botones de drogas con DataTrigger para highlight rojo cuando son sugeridos deben mantenerse funcionales

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 07-modificar-frontend-para-mostrar-botones-adrenalina-y-amiodarona-en-primera-vista-con-layout-optimizado*
*Context gathered: 2026-04-04*
