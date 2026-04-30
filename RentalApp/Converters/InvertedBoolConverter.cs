using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// XAML value converter that flips a boolean. Used in bindings where we need to show
/// something when a value is false, e.g. IsNotLoading = !IsLoading.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>Returns the logical NOT of the input bool. Returns false for non-bool input.</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }

    /// <summary>Two-way binding support — same logic as Convert.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }
}
