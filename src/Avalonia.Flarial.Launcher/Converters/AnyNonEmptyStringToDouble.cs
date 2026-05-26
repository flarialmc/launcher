using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Flarial.Launcher.Converters;

public class AnyNonEmptyStringToDouble : IMultiValueConverter
{
    public double TrueValue { get; set; } = 1.0;
    public double FalseValue { get; set; }

    public object Convert(
        IList<object?>? values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values == null || values.Count == 0)
            return FalseValue;

        // TRUE if any value is a non-empty string
        var anyNonEmpty = values.Any(v =>
            v is string s && !string.IsNullOrEmpty(s));

        return anyNonEmpty ? TrueValue : FalseValue;
    }
}