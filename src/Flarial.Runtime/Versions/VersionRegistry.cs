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

    sealed class GameVersionComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            GameVersion a = new(x!), b = new(y!);

            if (b._major != a._major)
                return b._major.CompareTo(a._major);

            if (b._minor != a._minor)
                return b._minor.CompareTo(a._minor);

            return b._build.CompareTo(a._build);
        }
    }

    static readonly GameVersionComparer s_comparer = new();

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
        var gameLaunchHelperTask = HttpService.GetBytesAsync(GameLaunchHelperUri);
        var supportedVersionsTask = HttpService.GetJsonAsync<Dictionary<string, bool>>(SupportedVersionsUri);
        var downloadLinksTask = HttpService.GetJsonAsync<Dictionary<string, Dictionary<string, string[]>>>(DownloadLinksUri);
        await Task.WhenAll(gameLaunchHelperTask, supportedVersionsTask, downloadLinksTask);

        var downloadLinks = await downloadLinksTask;
        var gameLaunchHelper = await gameLaunchHelperTask;
        var supportedVersions = await supportedVersionsTask;

        SortedDictionary<string, VersionEntry> items = new(s_comparer);
        foreach (var item in supportedVersions) items[item.Key] = new(item.Value);

        foreach (var item in downloadLinks["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var version = item.Key[..index];

            if (!items.TryGetValue(version, out var entry))
                continue;

            entry._item = new(version, item.Value, gameLaunchHelper);
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