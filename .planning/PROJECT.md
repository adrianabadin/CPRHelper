# ACLS Tracker

## What This Is

Aplicación móvil multiplataforma (Android e iOS) para seguimiento y guía de Reanimación Cardiopulmonar Avanzada (ACLS) según normas de la American Heart Association (AHA) edición 2020. Diseñada para uso tanto en entrenamiento de equipos como en emergencias reales de reanimación, proporciona un metrónomo con guía visual y auditiva para compresiones torácicas, recordatorios protocolizados basados en tiempo, y registro completo de eventos durante el código.

## Core Value

El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales, mejorando la adherencia al protocolo AHA ACLS 2020.

## Requirements

### Validated

- [x] **AUDI-01**: El metrónomo audible puede reproducir audio a 100-120 BPM (configurable) — Validado en Phase 1
- [x] **AUDI-02**: La visualización del metrónomo está sincronizada con el audio (animada, 60fps) — Validado en Phase 1
- [x] **TIME-01**: El sistema puede gestionar múltiples timers concurrentes (ciclos de 2 minutos + medicamentos) — Validado en Phase 1
- [x] **TIME-02**: Los timers pueden ser iniciados, pausados y reiniciados independientemente — Validado en Phase 1
- [x] **TIME-03**: Los timers muestran tiempo transcurrido en tiempo real — Validado en Phase 1

### Active

- [ ] Metrónomo que marca frecuencia de compresiones (100-120/min) con visualización digital y animación visual sincronizada
- [ ] Sonido que marca el ritmo del metrónomo
- [ ] Sistema de recordatorios por audio basados en protocolo ACLS 2020 (ej: "ya pasaron 2 minutos ¿quieres administrar adrenalina?", "¿descartaste las H y las T?")
- [ ] Interfaz para líder del código con botones en pantalla para confirmar acciones (medicamentos, ritmo, descartar H's y T's)
- [ ] Registro completo de todos los eventos durante reanimación (horarios de medicamentos, tiempos, acciones, ritmo, etc.)
- [ ] Funcionalidad offline completa sin conexión a internet
- [ ] Capacidad de exportar datos registrados en formato PDF y CSV cuando hay conexión
- [ ] Soporte para todos los algoritmos de ritmo ACLS: Fibrilación Ventricular/Taquicardia Ventricular (FV/TV), Actividad Eléctrica sin Pulso (AEA), Asistolia, Bradicardia, Taquicardia
- [ ] Arquitectura preparada para soportar múltiples roles en el futuro (actualmente solo rol de líder)

### Out of Scope

- Soporte para múltiples roles en v1 (compresor, aire/bolsa, registro, etc.) — preparará arquitectura pero rol único en v1
- Integración con sistemas hospitalarios en v1 — exportación manual en v1
- Gráficos de ritmo cardíaco en tiempo real — líder confirma ritmo manualmente
- Algoritmos de ritmo pediátrico en v1 — solo adultos ACLS 2020

## Context

Proyecto de aplicación móvil para profesionales de salud entrenados en ACLS. El usuario tiene experiencia previa con .NET MAUI y React Native, considerando ambos como frameworks potenciales. El contexto de uso incluye:

- Entornos hospitalarios y prehospitalarios donde la conectividad puede ser limitada
- Equipos de reanimación donde roles y responsabilidades están definidos según protocolos ACLS
- Necesidad de documentación precisa de eventos de reanimación para mejora de calidad y auditoría
- Adherencia estricta a protocolos AHA ACLS 2020 para garantizar calidad de atención

## Constraints

- **Framework**: .NET MAUI o React Native — debe seleccionarse uno al iniciar desarrollo
- **Plataformas**: Android e iOS — ambas requeridas
- **Conectividad**: Funcionalidad offline principal — no debe depender de conexión durante emergencia
- **Protocolo**: AHA ACLS 2020 — debe seguir las guías actualizadas de 2020 estrictamente
- **UI/UX**: Botones en pantalla — interfaz táctil optimizada para uso rápido en emergencias

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Rol único en v1 | Permite validar la funcionalidad central antes de expandir a multi-rol | — Pendiente |
| Registro completo | Necesario para documentación médica y mejora continua | — Pendiente |
| Exportación PDF + CSV | PDF para legibilidad, CSV para análisis de datos | — Pendiente |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 24/03/2026 after Phase 1 completion*