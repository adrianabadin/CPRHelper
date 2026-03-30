# Requirements: ACLS Tracker

**Defined:** 24/03/2026
**Core Value:** El líder del código puede guiar y registrar todo el evento de reanimación con apoyo protocolizado en tiempo real, sin depender de memoria o cálculos manuales

## v1 Requirements

Requirements para el lanzamiento inicial. Cada uno mapea a una fase del roadmap.

### Audio y Metrónomo

- [x] **AUDI-01**: El metrónomo audible puede reproducir audio a 100-120 BPM (configurable)
- [x] **AUDI-02**: La visualización del metrónomo está sincronizada con el audio (animada, 60fps)

### Gestión de Tiempo

- [x] **TIME-01**: El sistema puede gestionar múltiples timers concurrentes (ciclos de 2 minutos + medicamentos)
- [x] **TIME-02**: Los timers pueden ser iniciados, pausados y reiniciados independientemente
- [x] **TIME-03**: Los timers muestran tiempo transcurrido en tiempo real

### Registro de Eventos

- [x] **REGI-01**: El usuario puede seleccionar el ritmo actual (FV/TV, AEA, Asistolia, Bradicardia, Taquicardia)
- [x] **REGI-02**: El sistema registra eventos automáticamente con timestamps (incluyendo milisegundos)
- [x] **REGI-03**: El usuario puede marcar/descartar items de la lista de H's y T's
- [x] **REGI-04**: El sistema genera recordatorios contextuales basados en el protocolo ACLS 2020

### Exportación

- [x] **EXPO-01**: El usuario puede exportar datos de sesión en formato PDF (reporte legible)
- [x] **EXPO-02**: El usuario puede exportar datos de sesión en formato CSV (datos estructurados)

## v2 Requirements

Diferidos a una release futura. Rastreados pero no en el roadmap actual.

### Audio y Metrónomo

- **AUDI-03**: Los indicadores de estado muestran conectividad (online/offline) y ritmo actual en pantalla
- **AUDI-04**: Las sugerencias por audio son contextuales (ej: "¿hora de adrenalina?")

### Gestión de Tiempo

- [ ] **TIME-04**: Los recordatorios contextuales por audio se basan en el protocolo ACLS 2020 por ritmo específico

### Registro de Eventos

- [ ] **REGI-05**: El sistema sugiere automáticamente cuándo descartar H's y T's basándose en el tiempo y el contexto

### Funcionalidad Avanzada

- [ ] **SYNC-01**: El sistema soporta sincronización opcional con la nube cuando hay conectividad
- [ ] **HIST-01**: El usuario puede ver historial de sesiones anteriores con paginación
- [ ] **ROLE-01**: La arquitectura prepara el sistema para múltiples roles (compresor, aire, registrador, etc.)

## Out of Scope

Explícitamente excluido. Documentado para prevenir scope creep.

| Feature | Reason |
|---------|--------|
| Visualización adaptativa (urgencia color-codificada, alto contraste) | MVP simplificado. Diferenciador para v2+ |
| ECG Waveform display | Requiere clasificación de dispositivo médico FDA. Metrónomo es suficiente para MVP. |
| Voice commands | No confiable en entornos de emergencia ruidosos. Botones táctiles grandes preferidos. |
| Real-time ECG integration | Hardware complejo y certificación médica. Selección manual de ritmo es suficiente para v1. |
| Compartición social de datos | Violación de HIPAA. Datos de PHI no pueden compartirse casualmente. |
| Cloud-only storage | La app debe funcionar durante emergencias sin conectividad. Offline-first requerido. |

## Traceability

Qué fases cubren qué requisitos. Actualizado durante la creación del roadmap.

| Requirement | Phase | Status |
|-------------|-------|--------|
| AUDI-01 | Phase 1 | Complete |
| AUDI-02 | Phase 1 | Complete |
| TIME-01 | Phase 1 | Complete |
| TIME-02 | Phase 1 | Complete |
| TIME-03 | Phase 1 | Complete |
| REGI-01 | Phase 2 | Complete |
| REGI-02 | Phase 2 | Complete |
| REGI-03 | Phase 2 | Complete |
| REGI-04 | Phase 3 | Complete |
| EXPO-01 | Phase 5 | Complete |
| EXPO-02 | Phase 5 | Complete |

**Coverage:**
- v1 requirements: 11 total
- Mapped to phases: 11/11 ✓
- Unmapped: 0 ✓

---
*Requirements defined: 24/03/2026*
*Last updated: 24/03/2026 after roadmap creation*