namespace AclsTracker.Models;

public enum CardiacRhythm
{
    Ninguno,                    // No rhythm selected (initial state)
    FibrilacionVentricular,     // FV - Fibrilación Ventricular
    TaquicardiaVentricular,     // TV - Taquicardia Ventricular sin pulso
    ActividadElectricaSinPulso, // AEA - Actividad Eléctrica sin Pulso
    Asistolia,                  // Asistolia
    Bradicardia,                // Bradicardia
    Taquicardia                 // Taquicardia (con pulso - supraventricular/ventricular)
}
