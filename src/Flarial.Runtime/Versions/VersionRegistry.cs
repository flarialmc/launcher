using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

sealed class VersionEntry
{
    internal VersionItem? Item;
    internal readonly bool Supported;
    internal VersionEntry(bool supported) => Supported = supported;
}

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    static readonly VersionItemComparer s_comparer = new();

    const string SupportedVersionsUri = "https://cdn.flarial.xyz/launcher/Supported.json";

    readonly SortedDictionary<string, VersionEntry> _registry;

    VersionRegistry(string preferred, SortedDictionary<string, VersionEntry> registry)
    {
        _registry = registry;
        PreferredVersion = VersionItem.Stringify(preferred);
    }

    public readonly string PreferredVersion;

    public static string InstalledVersion => VersionItem.Stringify(Minecraft.Version);

    public bool IsSupported => _registry.TryGetValue(Minecraft.Version, out var entry) && entry.Supported;

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        SortedDictionary<string, VersionEntry> registry = new(s_comparer);

        using var stream = await HttpService.GetStreamAsync(SupportedVersionsUri);
        var json = await JsonService.ReadAsync<Dictionary<string, bool>>(stream);

        foreach (var item in json)
            registry.Add(item.Key, new(item.Value));

        await GDKVersionItem.QueryAsync(registry);
        var preferredVersion = registry.First(static _ => _.Value.Supported).Key;

        return new VersionRegistry(preferredVersion, registry);
    });


    public IEnumerator<VersionItem> GetEnumerator()
    {
        foreach (var value in _registry.Values)
        {
            if (value.Item is null) continue;
            yield return value.Item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    sealed class VersionItemComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            NumericVersion a = new(x), b = new(y);

            if (b.Major != a.Major)
                return b.Major.CompareTo(a.Major);

            if (b.Minor != a.Minor)
                return b.Minor.CompareTo(a.Minor);

            return b.Build.CompareTo(a.Build);
        }
    }
}