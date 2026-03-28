namespace AclsTracker.Models;

public enum CardiacRhythm
{
    Ninguno,    // No rhythm selected (initial state)
    RCE,        // Retorno de Circulacion Espontanea - green
    AESP,       // Actividad Electrica Sin Pulso - orange, non-shockable
    Asistolia,  // Asistolia - orange, non-shockable
    TV,         // Taquicardia Ventricular - red, shockable
    FV          // Fibrilacion Ventricular - red, shockable
}
