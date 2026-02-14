using System.IO;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Networking;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Json;

namespace Flarial.Launcher.Runtime.Versions;

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    internal sealed class VersionEntry
    {
        internal VersionItem? _item;
        internal readonly bool _supported;
        internal VersionEntry(bool supported) => _supported = supported;
    }

    static readonly VersionItemComparer s_comparer = new();
    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, bool>), VersionItem.s_settings);

    const string SupportedVersionsUrl = "https://cdn.flarial.xyz/launcher/Supported.json";

    readonly SortedDictionary<string, VersionEntry> _registry;

    VersionRegistry(string preferred, SortedDictionary<string, VersionEntry> registry)
    {
        _registry = registry;
        PreferredVersion = preferred;
    }

    public readonly string PreferredVersion;

    public static string InstalledVersion => NormalizeVersion(Minecraft.Version);

    public bool IsSupported => _registry.TryGetValue(Minecraft.Version, out var entry) && entry._supported;

    internal static string NormalizeVersion(string version)
    {
        VersionItemKey key = new(version);

        if (key._minor >= 26)
            return $"{key._minor}.{key._build}";

        return version;
    }

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        using var stream = await HttpService.GetStreamAsync(SupportedVersionsUrl);
        var items = (Dictionary<string, bool>)s_serializer.ReadObject(stream);

        SortedDictionary<string, VersionEntry> registry = new(s_comparer);
        foreach (var item in items) registry.Add(item.Key, new(item.Value));

        await GDKVersionItem.QueryAsync(registry);
        var preferred = registry.First(static _ => _.Value._supported).Key;

        return new VersionRegistry(preferred, registry);
    });


    public IEnumerator<VersionItem> GetEnumerator()
    {
        foreach (var value in _registry.Values)
        {
            if (value._item is null) continue;
            yield return value._item;
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