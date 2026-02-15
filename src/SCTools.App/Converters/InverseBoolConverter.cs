// Licensed to the SCTools project under the MIT license.

using System.Globalization;
using System.Windows.Data;

namespace SCTools.App.Converters;

/// <summary>
/// Inverts a <see cref="bool"/> value. True becomes False and vice versa.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not true;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not true;
    }
}
