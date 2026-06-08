using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    sealed class VersionEntry
    {
        internal VersionItem? _item;

        internal readonly bool _supported;

        internal VersionEntry(bool supported) => _supported = supported;
    }

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

    static readonly VersionItemComparer s_comparer = new();

    const string SupportedVersionsUri = "https://cdn.flarial.xyz/launcher/Supported.json";
    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";
    const string MSIXVCPackagesUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";

    readonly SortedDictionary<string, VersionEntry> _registry;

    VersionRegistry(SortedDictionary<string, VersionEntry> registry)
    {
        var preferred = (_registry = registry).First(static _ => _.Value._supported);
        PreferredVersion = VersionItem.Stringify(preferred.Key);
    }

    public readonly string PreferredVersion;

    public static string InstalledVersion => VersionItem.Stringify(Minecraft.Version);

    public bool IsSupported => _registry.TryGetValue(Minecraft.Version, out var entry) && entry._supported;

    public static async Task<VersionRegistry> GetAsync() => await Task.Run(static async () =>
    {
        var helperTask = HttpService.GetBytesAsync(GameLaunchHelperUri);
        var supportedTask = HttpService.GetJsonAsync<Dictionary<string, bool>>(SupportedVersionsUri);
        var packagesTask = HttpService.GetJsonAsync<Dictionary<string, Dictionary<string, string[]>>>(MSIXVCPackagesUri);

        SortedDictionary<string, VersionEntry> registry = new(s_comparer);
        foreach (var item in await supportedTask) registry.Add(item.Key, new(item.Value));

        foreach (var item in (await packagesTask)["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var key = item.Key.Substring(0, index);

            if (!registry.TryGetValue(key, out var entry)) continue;
            entry._item = new(key, item.Value, await helperTask);
        }

        return new VersionRegistry(registry);
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
}