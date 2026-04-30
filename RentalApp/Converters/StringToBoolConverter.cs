using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// XAML value converter that maps a non-empty string to true and empty/null to false.
/// Used to conditionally show UI elements based on whether a string property has content,
/// e.g. showing an error label only when ErrorMessage is non-empty.
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    /// <summary>Returns true if the string is non-null, non-empty, and non-whitespace.</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }

        return false;
    }

    /// <summary>
    /// ConvertBack maps true to "true" and false to empty string.
    /// Not commonly used but implemented for completeness.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "true" : string.Empty;
        }

        return string.Empty;
    }
}
