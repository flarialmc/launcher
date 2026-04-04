using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    internal sealed class VersionEntry
    {
        internal VersionItem? _item;
        internal readonly bool _supported;
        internal VersionEntry(bool supported) => _supported = supported;
    }

    static readonly VersionItemComparer s_comparer = new();
    static readonly JsonSerializer<Dictionary<string, bool>> s_serializer = JsonSerializer<Dictionary<string, bool>>.Get();

    const string SupportedVersionsUri = "https://cdn.flarial.xyz/launcher/Supported.json";

    readonly SortedDictionary<string, VersionEntry> _registry;

    VersionRegistry(string preferred, SortedDictionary<string, VersionEntry> registry)
    {
        _registry = registry;
        PreferredVersion = VersionItem.Stringify(preferred);
    }

    public readonly string PreferredVersion;

    public static string InstalledVersion => VersionItem.Stringify(Minecraft.Version);

    public bool IsSupported => _registry.TryGetValue(Minecraft.Version, out var entry) && entry._supported;

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        SortedDictionary<string, VersionEntry> registry = new(s_comparer);
        using var stream = await HttpStack.GetStreamAsync(SupportedVersionsUri);

        foreach (var item in s_serializer.Deserialize(stream))
            registry.Add(item.Key, new(item.Value));

        await GDKVersionItem.QueryAsync(registry);
        var preferredVersion = registry.First(static _ => _.Value._supported).Key;

        return new VersionRegistry(preferredVersion, registry);
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

    sealed class VersionItemComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            NumericVersion a = new(x), b = new(y);

            if (b._major != a._major)
                return b._major.CompareTo(a._major);

            if (b._minor != a._minor)
                return b._minor.CompareTo(a._minor);

            return b._build.CompareTo(a._build);
        }
    }
}