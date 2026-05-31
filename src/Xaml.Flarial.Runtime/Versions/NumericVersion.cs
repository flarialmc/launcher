namespace Flarial.Runtime.Versions;

unsafe readonly struct NumericVersion
{
    internal NumericVersion(string version)
    {
        var index = 0;
        var segments = stackalloc int[3];

        foreach (var value in version) if (value is '.') ++index;
        else segments[index] = value - '0' + segments[index] * 10;

        Major = segments[0];
        Minor = segments[1];
        Build = segments[2];
    }

    internal readonly int Major { get; }
    internal readonly int Minor { get; }
    internal readonly int Build { get; }
}