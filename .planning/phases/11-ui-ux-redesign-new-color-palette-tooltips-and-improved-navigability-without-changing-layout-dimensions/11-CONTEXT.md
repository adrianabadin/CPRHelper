# Phase 11: UI/UX Redesign — Context

**Gathered:** 2026-04-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Rediseño visual y de experiencia de usuario de la app ACLS Tracker: nueva paleta de colores clínica/profesional, sistema de tooltips informativos con iconos ⓘ, y mejoras de navegabilidad (remover EventLogPanel de MainPage, tab bar colapsable). No se cambian dimensiones de layout existentes ni se agregan funcionalidades nuevas de protocolo.

</domain>

<decisions>
## Implementation Decisions

### Paleta de colores — Dirección visual
- Estética **clínica/profesional**: blancos limpios, azules/grises apagados, acentos sutiles. Sensación de equipamiento hospitalario.
- **Light-mode como default**, dark mode soportado
- Colores semánticos de ritmo **preservados** (rojo=desfibrilable TV/FV, amarillo=no desfibrilable AESP/Asistolia, verde=RCE), pero mejorar tonos específicos, sombras y bordes de selección
- **Consolidar todos los colores en un ResourceDictionary central** con estilos nombrados (ej: `ActionButtonPrimary`, `TimerCardBackground`, `RhythmShockable`, etc.) — eliminar colores hardcodeados inline en XAML

### Tooltips — Contenido y comportamiento
- Contenido: **protocolo + uso** combinados (ej: qué significa FCT%, qué hace cada timer, qué significan las categorías de ritmo, info de dosificación, cómo usar botones)
- Trigger: **icono info (ⓘ)** pequeño junto a elementos que tienen tooltip — descubrible sin ocupar demasiado espacio
- Dismiss: **auto-dismiss 3-4 segundos + tap para cerrar** antes
- Disponibilidad: **siempre disponibles**, incluso durante códigos activos

### Navegabilidad — EventLogPanel
- **Remover EventLogPanel de MainPage** completamente — los eventos se siguen registrando internamente pero el feed en vivo desaparece de la pestaña Principal
- Los eventos se visualizan desde la pestaña **Historial**
- Esto libera espacio vertical significativo en MainPage (el EventLog era el último bloque, debajo del fold)

### Navegabilidad — Tab bar colapsable
- El **bottom Shell tab bar se oculta por defecto** para maximizar espacio de pantalla
- Se revela mediante **gesto o toggle** (swipe up, o botón pequeño)
- Las 3 pestañas se mantienen: Principal, Causas Reversibles, Historial
- No se renombran ni se agregan/eliminan pestañas

### Claude's Discretion
- Selección exacta de colores de la paleta clínica (tonos específicos de azul, gris, blanco)
- Implementación técnica del ResourceDictionary (App.xaml vs archivo separado, estructura de naming)
- Diseño visual exacto del icono ⓘ (tamaño, posicionamiento, color)
- Implementación del tooltip (custom popup, overlay, ContentView)
- Mecanismo técnico para ocultar/mostrar el tab bar (Shell.TabBarIsVisible, custom renderer, gesture recognizer)
- Qué elementos específicos reciben tooltip (priorizar los más útiles)
- Transición/animación al mostrar/ocultar tab bar

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `App.xaml`: Ya tiene ResourceDictionary con converters globales (IsNotNullConverter, BoolToColorConverter). Punto natural para centralizar colores y estilos.
- `AppShell.xaml`: Define la navegación Shell con 3 tabs. TabBarIsVisible es una propiedad de Shell que se puede bindear.
- `NotificationBanner.xaml`: Patrón existente de overlay con auto-dismiss que puede informar el diseño de tooltips.

### Current Color Map (to be consolidated)
- Page background: `#FFFFFF` / `#121212`
- TimerCard bg: `#F5F5F5` / `#1E1E1E`, border: `#E0E0E0` / `#333333`
- INICIAR CODIGO: `#E65100`, FINALIZAR: `#757575`
- Metronome circle: `#D32F2F`, toggle: `#1976D2`/`#1565C0`
- BPM +/- buttons: `#E0E0E0`/`#333333`
- RCE: `#2E7D32`, AESP/Asistolia: `#FBC02D`, TV/FV: `#D32F2F`
- NUEVO CICLO: `#FF9800`, DEFIBRILAR: `#C62828`
- ADRENALINA/AMIODARONA: `#7B1FA2`, suggested: `#D32F2F`
- Banner: `#FFF3C2` bg, `#333333` text
- ExtraInfo: `#999999`
- Timer text: `#111111` / `#FFFFFF`
- Over threshold: `#FFEBEE` bg, `#D32F2F` text
- Active rhythm border: Black, 3px

### Established Patterns
- `AppThemeBinding` for light/dark mode switching — continue using this
- `DataTrigger` for state-based color changes (rhythm selection, threshold alerts, drug suggestions)
- `Frame` with CornerRadius for card-style containers
- Controls use `BindingContext` directly to ViewModels

### Integration Points
- `App.xaml` ResourceDictionary: Add named Color and Style resources
- `MainPage.xaml`: Remove EventLogPanel reference (Block 6)
- `AppShell.xaml`: Add tab bar visibility toggle mechanism
- All 7 controls + 9 views: Replace inline colors with StaticResource references
- New `TooltipView` control or similar for tooltip rendering
- `MainViewModel` or new VM: Tab bar visibility state

### Files to Modify
- `App.xaml` — ResourceDictionary with color palette + styles
- `MainPage.xaml` — remove EventLogPanel, update colors to resources
- `AppShell.xaml` — collapsible tab bar
- `TimerCard.xaml` — colors → resources, add ⓘ icon
- `MetronomePulse.xaml` — colors → resources, add ⓘ icon
- `RhythmSelector.xaml` — colors → resources, improve shadows/borders, add ⓘ
- `NotificationBanner.xaml` — colors → resources
- `EventLogPanel.xaml` — possibly integrate into HistorialPage
- All action buttons in MainPage — colors → resources

</code_context>

<specifics>
## Specific Ideas

- Estética de equipamiento hospitalario — limpia, profesional, confiable
- Colores semánticos de ritmo son intocables en significado (rojo/amarillo/verde), pero se pueden refinar los tonos
- Los tooltips deben ayudar tanto a usuarios nuevos como a recordar detalles de protocolo durante el uso
- El tab bar oculto maximiza pantalla — crítico para dispositivos con pantallas pequeñas durante emergencias
- Remover el EventLog de Principal simplifica la vista principal y fuerza consulta en Historial

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 11-ui-ux-redesign-new-color-palette-tooltips-and-improved-navigability-without-changing-layout-dimensions*
*Context gathered: 2026-04-12*
