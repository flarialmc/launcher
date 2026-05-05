using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Flarial.Launcher.Converters;

public class RoundToWholeNumber : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return double.TryParse(value.ToString(), out var number) ? Math.Round(number, MidpointRounding.AwayFromZero) : value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return double.TryParse(value.ToString(), out double number) ? Math.Round(number, MidpointRounding.AwayFromZero) : value;
    }
}