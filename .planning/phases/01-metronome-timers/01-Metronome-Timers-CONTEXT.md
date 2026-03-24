# Phase 1: Metronome & Timers - Context

**Gathered:** 24/03/2026
**Status:** Ready for planning

## Phase Boundary

Usuarios pueden gestionar funciones core de tiempo para guía de reanimación cardiaca. Esta fase establece el fundamento de temporización (metrónomo) y gestión de temporizadores (ciclos de 2 minutos + medicamentos) sobre el cual se construirán capacidades de registro y recordatorios en fases posteriores.

## Implementation Decisions

### Arquitectura del Metrónomo

**D-01:** Usar Stopwatch de .NET con alta precisión

**Why this way:** La investigación (.planning/research/STACK.md) recomienda .NET MAUI sobre React Native porque la compilación a nativos proporciona precisión superior de tiempo crítico. La clase Stopwatch de .NET ofrece precisión de microsegundos, necesaria para mantener ±1 BPM.

**If wrong:** Usar callbacks de audio simples sin sincronización de alta precisión → El metrónomo podría derivar del ritmo objetivo, afectando la calidad de RCP.

---

### Implementación de Audio

**D-02:** Usar Plugin.Maui.Audio con configuración optimizada

**Why this way:** La investigación (.planning/research/STACK.md) especifica Plugin.Maui.Audio como la abstracción cross-platform recomendada. Este plugin maneja las diferencias de iOS y Android, proporcionando una API consistente para reproducir audio con latencia mínima.

**If wrong:** Usar APIs nativas específicas de cada plataforma → Aumenta la complejidad del código, mayor mantenimiento, y riesgo de inconsistencia entre plataformas.

---

### Visualización del Metrónomo

**D-03:** Pulso animado (círculo que late con cada beep)

**Why this way:** Los requisitos (.planning/REQUIREMENTS.md) especifican visualización animada sincronizada con audio a 60fps. Un pulso visual que se expande/contrac con cada compresoión es intuitivo y claramente visible en condiciones de emergencia.

**If wrong:** Usar solo un contador digital sin animación → Los usuarios no verían el ritmo visualmente, solo escucharían el audio, lo cual es menos efectivo para mantener el ritmo.

---

### Sistema de Timers

**D-04:** Timers independientes con observable collections separadas, pero todos visibles en misma pantalla

**Why this way:** Los requisitos (.planning/REQUIREMENTS.md) especifican que los usuarios deben poder gestionar múltiples timers concurrentes (TIME-02). La decisión del usuario requiere que todos los timers se muestren simultáneamente en pantalla, pero cada uno pueda iniciarse, pausarse y reiniciarse independientemente.

**If wrong:** Usar un coordinador central que oculta la gestión de los timers individuales → Los usuarios no podrían ver todos los temporizadores simultáneamente en una pantalla, complicando la toma de decisiones durante emergencias.

---

### Patrón de Visualización de Timers

**D-05:** Formato digital clásico (00:00) con círculo de progreso

**Why this way:** La investigación (.planning/research/PITFALLS.md) advierte sobre interfaces complejas en emergencias. Un formato digital clásico con indicadores circulares de progreso es fácil de leer en condiciones de estrés, con alto contraste y claridad.

**If wrong:** Usar formato analógico complejo o elementos visuales pequeños → Los usuarios podrían confundirse o no poder leer la información rápidamente durante una emergencia cardíaca.

---

### agent's Discretion

Ninguna área delegada explícitamente al agente. Todas las decisiones técnicas están capturadas arriba.

---

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Metrónomo y Sistema de Audio
- `.planning/research/STACK.md` — Recomendación de tecnología (.NET MAUI, Plugin.Maui.Audio)
- `.planning/research/PITFALLS.md` — Advertencias sobre precisión de tiempo y latencia de audio

### Visualización
- `.planning/REQUIREMENTS.md` — Especificación de AUDI-02 (visualización sincronizada a 60fps)
- `.planning/research/ARCHITECTURE.md` — Patrones MVVM y optimizaciones con CommunityToolkit.Maui

### Gestión de Timers
- `.planning/REQUIREMENTS.md` — Especificación de TIME-01, TIME-02, TIME-03 (gestión de múltiples timers)
- `.planning/research/ARCHITECTURE.md` — Patrón de Repositorio y ObservableCollection para timers

---

*Phase: 01-Metronome-Timers*