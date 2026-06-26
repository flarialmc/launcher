using System;
using Windows.ApplicationModel;

namespace Flarial.Runtime.Versions;

unsafe readonly ref struct GameVersion
{
    internal GameVersion(string version)
    {
        var index = 0;
        var segments = stackalloc int[3];

        foreach (var character in version)
        {
            if (character is '.')
            {
                if (++index >= 3)
                    throw new FormatException();
                continue;
            }

            var digit = character - '0';
            if (digit > 9) throw new FormatException();
            segments[index] = segments[index] * 10 + digit;
        }

        _major = segments[0];
        _minor = segments[1];
        _build = segments[2];
    }

    internal GameVersion(PackageVersion version)
    {
        _major = version.Major;
        _minor = version.Minor;
        _build = version.Build / 100;
    }

    internal readonly int _major, _minor, _build;

    public override string ToString()
    {
        if (_minor >= 26) return $"{_minor}.{_build}";
        return $"{_major}.{_minor}.{_build}";
    }
}