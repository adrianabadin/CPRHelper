using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Converts bool to Color. True = Green (running), False = Gray (stopped).
/// Used by TimerCard running indicator dot.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return Colors.LimeGreen;
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}