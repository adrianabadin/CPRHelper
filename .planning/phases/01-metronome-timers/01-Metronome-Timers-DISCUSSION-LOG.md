# Phase 1: Metronome & Timers - Discussion Log

**Date:** 24/03/2026
**Phase:** 01-Metronome-Timers
**Mode:** Assumptions (codebase-first analysis)
**Areas discussed:** 5 areas

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves alternatives considered.

---

## Arquitectura del Metrónomo

| Option | Description | Selected |
|---------|-------------|----------|
| Stopwatch .NET con alta precisión | Usar Stopwatch de .NET con callbacks de audio para lograr ±1 BPM | ✓ |

**User's choice:** Stopwatch .NET con alta precisión

**Notes:** Usuario seleccionó la recomendación de la investigación (.NET MAUI con Stopwatch de alta precisión) para garantizar la precición de tiempo crítico en el metrónomo.

---

## Implementación de Audio

| Option | Description | Selected |
|---------|-------------|----------|
| Plugin.Maui.Audio con configuración optimizada | Abstracción cross-platform que maneja diferencias iOS/Android | ✓ |

**User's choice:** Plugin.Maui.Audio

**Notes:** Consistente con la recomendación del stack (.planning/research/STACK.md). Plugin.Maui.Audio es la abstracción cross-platform recomendada para apps .NET MAUI.

---

## Visualización del Metrónomo

| Option | Description | Selected |
|---------|-------------|----------|
| Pulso animado (círculo que late con cada beep) | Visualización sincronizada con audio, intuitiva y claramente visible | ✓ |

**User's choice:** Pulso animado

**Notes:** Cumple con el requisito AUDI-02 (.planning/REQUIREMENTS.md) de visualización sincronizada a 60fps. Un pulso que se expande/contrac con cada compresoión es ideal para condiciones de emergencia.

---

## Sistema de Timers

| Option | Description | Selected |
|---------|-------------|----------|
| Timers independientes con observable collections separadas, pero todos visibles en misma pantalla | Cada timer tiene su propia ObservableCollection, pero el usuario puede ver todos simultáneamente | ✓ |

**User's choice:** Timers independientes visibles en pantalla

**Notes:** Aclara aclaración del usuario: "NO ENTIENDO QUE DIFERENCIA HAY ENTRE LAS OPCIONES, QUIERO QUE LOS TIMERS DE DROGAS CICLOS Y TIEMPO TOTAL Y TIEMPO DE COMPRESIONES ESTEN TODOS EN LA MISMA PANTALLA PERO QUE SE PUEDAN INICIAR PAUSAR Y REINICIAR INDEPENDIENTEMENTE". Esto requiere que todos los timers sean observables independientemente para UI binding, pero visibles en la misma pantalla.

---

## agent's Discretion

Ninguna área delegada explícitamente al agente. Todas las decisiones técnicas están capturadas arriba.

---

## Deferred Ideas

Ninguna idea diferida durante esta discusión.

---

*Phase: 01-Metronome-Timers*
*Discussion logged: 24/03/2026*