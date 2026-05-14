#if NETFRAMEWORK
#pragma warning disable IDE0130

namespace System.Runtime.CompilerServices
{
    static class IsExternalInit { }
}

namespace System
{
    static class MathExtensions
    {
        extension(Math)
        {
            public static double Clamp(double value, double min, double max)
            {
                if (value < min) return min;
                else if (value > max) return max;
                return value;
            }

        }
    }

    static class StringExtensions
    {
        extension(string value)
        {
            public string Replace(string oldValue, string newValue, StringComparison comparisonType)
            {
                _ = comparisonType;
                return value.Replace(oldValue, newValue);
            }
        }
    }
}

#pragma warning restore IDE0130
#endif