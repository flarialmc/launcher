using System.Collections.Generic;

namespace Flarial.Runtime.Versions;

sealed class VersionItemComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        VersionKey a = new(x!), b = new(y!);

        if (b._major != a._major)
            return b._major.CompareTo(a._major);

        if (b._minor != a._minor)
            return b._minor.CompareTo(a._minor);

        return b._build.CompareTo(a._build);
    }
}