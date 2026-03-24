using System.Globalization;

namespace AclsTracker.Converters;

/// <summary>
/// Returns true when value is not null. Used to show/hide progress bar
/// based on whether TargetDuration is set (D-05).
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}