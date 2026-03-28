using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Converts a bool to opacity: true -> 1.0 (fully visible), false -> 0.4 (dimmed/disabled).
/// Used for the AMIODARONA button which is only enabled when rhythm is TV or FV.
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 1.0 : 0.4;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
