using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Flarial.Launcher.Services.Versions;

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    internal sealed class VersionEntry
    {
        internal VersionItem? _value;
        internal VersionEntry() { }
    }

    static readonly VersionItemComparer s_comparer = new();

    readonly IReadOnlyDictionary<string, VersionEntry> _registry;

    VersionRegistry(IReadOnlyDictionary<string, VersionEntry> registry)
    {
        _registry = registry;
    }

    internal static string NormalizeVersion(string version)
    {
        VersionItemKey key = new(version);

        if (key._minor >= 26)
            return $"{key._minor}.{key._build}";

        return version;
    }

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        SortedDictionary<string, VersionEntry> registry = new(s_comparer);

        var uwp = UWPVersionItem.QueryAsync(registry);
        var gdk = GDKVersionItem.QueryAsync(registry);
        await Task.WhenAll(uwp, gdk);

        return new VersionRegistry(registry);
    });


    public IEnumerator<VersionItem> GetEnumerator()
    {
        foreach (var value in _registry.Values)
        {
            if (value._value is null) continue;
            yield return value._value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /*
        - This might be a "micro-optimization".
        - We can avoid using `System.Version` to avoid potential overhead.
    */

    unsafe readonly struct VersionItemKey
    {
        internal VersionItemKey(string version)
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

    sealed class VersionItemComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            VersionItemKey a = new(x), b = new(y);

            if (b._major != a._major)
                return b._major.CompareTo(a._major);

            if (b._minor != a._minor)
                return b._minor.CompareTo(a._minor);

            return b._build.CompareTo(a._build);
        }
    }
}