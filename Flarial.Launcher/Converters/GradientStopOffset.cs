using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Flarial.Launcher.Converters;

public class GradientStopOffset() : IValueConverter
{
    /// <summary>
    /// How much to shift left/right from the center value.
    /// Default: 0.0125 (1.25%)
    /// </summary>
    public double OffsetAmount { get; set; } = 0.0125;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return 0d;

        if (!double.TryParse(value.ToString(), out var center))
            return 0d;

        center *= 0.01;
        
        var direction = parameter?.ToString()?.ToLowerInvariant();

        var result = direction switch
        {
            "left" => center - OffsetAmount,
            "right" => center + OffsetAmount,
            _ => center
        };
        
        result = Math.Min(Math.Max(result, 0d), 1d);

        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
