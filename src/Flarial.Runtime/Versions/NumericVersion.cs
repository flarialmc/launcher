namespace Flarial.Runtime.Versions;

unsafe readonly struct NumericVersion
{
    internal NumericVersion(string version)
    {
        var index = 0;
        var segments = stackalloc int[3];

        foreach (var value in version) if (value is '.') ++index;
        else segments[index] = value - '0' + segments[index] * 10;

        _major = segments[0];
        _minor = segments[1];
        _build = segments[2];
    }

    internal readonly int _major, _minor, _build;
}