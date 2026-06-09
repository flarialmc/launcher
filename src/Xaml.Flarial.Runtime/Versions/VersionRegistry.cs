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
    const string DownloadLinksUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";

    readonly SortedDictionary<string, VersionEntry> _items;

    VersionRegistry(SortedDictionary<string, VersionEntry> items)
    {
        _items = items;
        PreferredVersion = VersionItem.Stringify(items.First(static _ => _.Value._supported).Key);
    }

    public readonly string PreferredVersion;

    public static string InstalledVersion => VersionItem.Stringify(Minecraft.Version);

    public bool IsSupported => _items.TryGetValue(Minecraft.Version, out var entry) && entry._supported;

    public static async Task<VersionRegistry> GetAsync() => await Task.Run(static async () =>
    {
        var gameLaunchHelper = HttpService.GetBytesAsync(GameLaunchHelperUri);
        var supportedVersions = HttpService.GetJsonAsync<Dictionary<string, bool>>(SupportedVersionsUri);
        var downloadLinks = HttpService.GetJsonAsync<Dictionary<string, Dictionary<string, string[]>>>(DownloadLinksUri);
        await Task.WhenAll(gameLaunchHelper, supportedVersions, downloadLinks);

        SortedDictionary<string, VersionEntry> items = new(s_comparer);

        foreach (var item in await supportedVersions)
            items[item.Key] = new(item.Value);

        foreach (var item in (await downloadLinks)["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var version = item.Key.Substring(0, index);

            if (!items.TryGetValue(version, out var entry))
                continue;

            entry._item = new(version, item.Value, await gameLaunchHelper);
        }

        return new VersionRegistry(items);
    });

    public IEnumerator<VersionItem> GetEnumerator()
    {
        foreach (var value in _items.Values)
        {
            if (value._item is null) continue;
            yield return value._item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}