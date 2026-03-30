using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Converts bool to active/inactive background Color for toggle buttons.
/// True (active) = Blue #1565C0, False (inactive) = Light Gray #E0E0E0.
/// Used by HistorialPage toggle buttons (Sesión Actual / Sesiones Guardadas).
/// </summary>
public class BoolToActiveColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return Color.FromArgb("#1565C0");
        return Color.FromArgb("#E0E0E0");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
