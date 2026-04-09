using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Converts bool to "ON"/"OFF" string for toggle button labels.
/// Used by MetronomePulse toggle button.
/// </summary>
public class BoolToOnOffConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "ON" : "OFF";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
