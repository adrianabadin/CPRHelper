# Phase 10: Agregar datos extra en timecards - Context

**Gathered:** 2026-04-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Agregar datos informativos secundarios a las tarjetas TimerCard existentes: número de ciclo en Ciclo RCP, fracción de compresión torácica (FCT%) en T.Comp, y número de dosis en Adrenalina y Amiodarona. Los datos se muestran en estilo visual sutil (más chico y más claro que el contador de tiempo). No se agregan nuevas funcionalidades, solo información visual en los timers existentes.

</domain>

<decisions>
## Implementation Decisions

### Ubicación del dato extra
- Dato a la derecha del tiempo transcurrido (mm:ss), en la misma fila existente (Row 1 del Grid actual)
- NO se agregan filas nuevas ni se aumenta la altura del TimerCard
- El dato se oculta (IsVisible=false) cuando no hay valor (valor = 0 o no aplica)
- Timers sin dato extra: Tiempo Total y T. Pulsos

### Estilo visual sutil
- FontSize: 12pt, FontAttributes: Normal (no bold)
- Color: gris #999999 (mismo color en light y dark mode)
- Todos los datos extra comparten exactamente el mismo estilo
- El tiempo transcurrido sigue siendo 20pt Bold #111111/#FFFFFF

### Datos a mostrar por timer
- **Ciclo RCP** (Timers[1]): número de ciclo entero (solo el número, sin prefijo)
- **T.Comp** (Timers[2]): fracción de compresión torácica como porcentaje (solo el número + "%", ej: "67%")
- **Adrenalina** (Timers[3]): número de dosis entero (solo el número, sin prefijo)
- **Amiodarona** (Timers[4]): número de dosis entero (solo el número, sin prefijo)
- **Tiempo Total** (Timers[0]): sin dato extra
- **T. Pulsos** (Timers[5]): sin dato extra

### Contador de dosis adrenalina
- Nuevo contador acumulativo `_adrenalinaDoseCount` en MainViewModel (igual que `_amiodaronaDoseCount`)
- Se incrementa cada vez que se ejecuta el comando Adrenalina
- Se resetea solo en NUEVO CODIGO (ResetCodeState), NO al CONTINUAR
- Consistente con el comportamiento existente de amiodarona

### Regla de visibilidad (ocultar en 0)
- Todos los datos extra se ocultan cuando valor = 0
- Ciclo: oculto cuando _cycleCount = 0 (aparece "1" después del primer NewCycle)
- Adrenalina: oculto cuando _adrenalinaDoseCount = 0 (aparece "1" después de primera dosis)
- Amiodarona: oculto cuando _amiodaronaDoseCount = 0 (aparece "1" después de primera dosis)
- FCT: siempre visible cuando el timer de compresiones está corriendo (mostrar 0% es válido)

### Claude's Discretion
- Cómo pasar los datos del MainViewModel al TimerModel para binding (BindableProperty, propiedad directa en TimerModel, o converter)
- Cómo calcular la FCT: (tiempo de compresiones acumulado / tiempo total transcurrido) * 100
- Formato exacto del FCT% (¿0% o 0.0%?)
- Layout XAML exacto para acomodar el dato a la derecha del tiempo sin desbordar

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `TimerModel.cs`: Modelo con [ObservableProperty] para binding reactivo. Agregar propiedad `ExtraInfo` (string) con ObservableProperty permitiría binding directo desde XAML.
- `TimerCard.xaml`: Grid 3x2 con ColumnDefinitions="*, Auto". Row 1 tiene tiempo (Column 0) y botón pausa (Column 1). Se puede agregar un Label entre ambos o usar un StackLayout horizontal.
- `MainViewModel._cycleCount`: Ya existe, es privado. Hay que exponerlo al TimerModel.
- `MainViewModel._amiodaronaDoseCount`: Ya existe y funciona como contador. Patrón a seguir para adrenalina.

### Established Patterns
- CommunityToolkit.Mvvm [ObservableProperty] para propiedades observables en TimerModel
- BindableProperty en TimerCard.xaml.cs para propiedades custom del control
- El TimerCard usa BindingContext directo al TimerModel (BindingContext="{Binding Timer.Timers[N]}")
- Los timers se acceden por índice: Timers[0]=Total, [1]=Ciclo, [2]=Compresiones, [3]=Adrenalina, [4]=Amiodarona, [5]=Pulsos

### Integration Points
- `TimerModel.cs`: Agregar propiedad ExtraInfo (string observable) que el XAML pueda bindear
- `TimerCard.xaml`: Agregar Label condicional (IsVisible cuando ExtraInfo no es vacío) en Row 1, Column 0 o entre tiempo y pausa
- `MainViewModel.cs`: Agregar `_adrenalinaDoseCount`, actualizar TimerModel.ExtraInfo cuando cambian ciclo/dosis/FCT
- `MainViewModel.NewCycle()`: Actualizar ExtraInfo de Timers[1] con _cycleCount
- `MainViewModel.Adrenalina()`: Incrementar _adrenalinaDoseCount y actualizar ExtraInfo de Timers[3]
- `MainViewModel.Amiodarona()`: Actualizar ExtraInfo de Timers[4] con _amiodaronaDoseCount
- `MainViewModel.ResetCodeState()`: Resetear _adrenalinaDoseCount y limpiar ExtraInfo de todos los timers
- `TimerService.UpdateTimerValues()`: Posible lugar para calcular FCT en cada tick (~20Hz), o enlazar al OnElapsedChanged de TimerModel

</code_context>

<specifics>
## Specific Ideas

- "Solo los números enteros y en el caso de la FCT agregar el % nada más para que entre en la pantalla"
- "El agregado del dato no debe aumentar el número de filas ni la altura de los timecards"
- Estilo sutil: gris #999999 a 12pt, claramente secundario vs el tiempo principal a 20pt Bold

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 10-agregar-datos-extra-en-timecards-numero-de-ciclo-fraccion-de-compresion-en-t-comp-numero-de-dosis-en-adrenalina-y-amiodarona*
*Context gathered: 2026-04-11*
