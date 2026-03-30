using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Converts bool to active/inactive text Color for toggle buttons.
/// True (active) = White, False (inactive) = Dark Gray #666666.
/// Used by HistorialPage toggle buttons (Sesión Actual / Sesiones Guardadas).
/// </summary>
public class BoolToActiveTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return Colors.White;
        return Color.FromArgb("#666666");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
