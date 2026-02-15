// Licensed to the SCTools project under the MIT license.

using System.Globalization;
using System.Windows.Data;

namespace SCTools.App.Converters;

/// <summary>
/// Converts a nullable value to <see cref="bool"/>. Non-null = True, Null = False.
/// </summary>
[ValueConversion(typeof(object), typeof(bool))]
public sealed class NullToBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("NullToBoolConverter does not support ConvertBack.");
    }
}
