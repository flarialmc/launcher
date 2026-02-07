using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Networking;
using System.Linq;

namespace Flarial.Launcher.Services.Versions;

public sealed class VersionRegistry : IEnumerable<KeyValuePair<string, VersionItem>>
{
    internal sealed class VersionEntry
    {
        internal VersionItem? _item;
        internal readonly bool _supported;
        internal VersionEntry(bool supported) => _supported = supported;
    }

    static readonly VersionItemComparer s_comparer = new();

    const string SupportedVersionsUrl = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly IReadOnlyDictionary<string, VersionEntry> _registry;

    VersionRegistry(string preferred, IReadOnlyDictionary<string, VersionEntry> registry)
    {
        _registry = registry;
        Preferred = preferred;
    }

    public readonly string Preferred;

    public bool Supported => _registry.TryGetValue(Minecraft.Version, out var entry) && entry._supported;

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        SortedDictionary<string, VersionEntry> registry = new(s_comparer);
        using var stream = await HttpService.GetStreamAsync(SupportedVersionsUrl);

        string value = string.Empty;
        using StreamReader reader = new(stream);

        while ((value = await reader.ReadLineAsync()) is { })
        {
            value = value.Trim();

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                continue;

            registry.Add(value.Trim(), new(true));
        }

        var preferred = registry.Keys.First();
        var uwp = UWPVersionItem.QueryAsync(registry);
        var gdk = GDKVersionItem.QueryAsync(registry);
        await Task.WhenAll(uwp, gdk);

        return new VersionRegistry(preferred, registry);
    });

    /*
        - This might be a "micro-optimization".
        - We can avoid using `System.Version` to avoid potential overhead.
    */

    sealed class VersionItemComparer : IComparer<string>
    {
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionItem>> GetEnumerator()
    {
        foreach (var entry in _registry)
        {
            if (entry.Value._item is null) continue;
            yield return new(entry.Key, entry.Value._item);
        }
    }
}