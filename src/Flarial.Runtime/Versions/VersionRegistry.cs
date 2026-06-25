using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using Windows.ApplicationModel;
using Windows.Devices.Midi;

namespace Flarial.Runtime.Versions;

public sealed class VersionRegistry : IEnumerable<VersionItem>
{
    sealed class VersionItemComparer : IComparer<VersionItem>
    {
        public int Compare(VersionItem? x, VersionItem? y)
        {
            GameVersion a = new(x!._version!);
            GameVersion b = new(y!._version);

            if (b._major != a._major)
                return b._major.CompareTo(a._major);

            if (b._minor != a._minor)
                return b._minor.CompareTo(a._minor);

            return b._build.CompareTo(a._build);
        }
    }

    static string RoundVersionBuild(in GameVersion version)
    {
        var build = version._build / 10 * 10;
        return $"{version._major}.{version._minor}.{build}";
    }

    public static string InstalledVersion
    {
        get
        {
            var version = Minecraft.Package.Id.Version;
            return new GameVersion(version).ToString();
        }
    }

    static readonly VersionItemComparer s_comparer = new();

    const string SupportedVersionsUri = "https://cdn.flarial.xyz/launcher/Versions.json";
    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";
    const string DownloadLinksUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";

    readonly List<VersionItem> _versionItems;
    readonly HashSet<string> _supportedVersions;

    VersionRegistry(HashSet<string> supportedVersions, List<VersionItem> versionItems)
    {
        _versionItems = versionItems;
        _supportedVersions = supportedVersions;
        PreferredVersion = $"{_versionItems[0]}";
    }

    public string PreferredVersion { get; }

    public bool IsSupported
    {
        get
        {
            var packageVersion = Minecraft.Package.Id.Version;
            GameVersion gameVersion = new(packageVersion);

            var roundedVersion = RoundVersionBuild(gameVersion);
            return _supportedVersions.Contains(roundedVersion);
        }
    }

    public static Task<VersionRegistry> GetAsync() => Task.Run(static async () =>
    {
        var gameLaunchHelperTask = HttpService.GetBytesAsync(GameLaunchHelperUri);
        var supportedVersionsTask = HttpService.GetJsonAsync<HashSet<string>>(SupportedVersionsUri);
        var downloadLinksTask = HttpService.GetJsonAsync<Dictionary<string, Dictionary<string, string[]>>>(DownloadLinksUri);

        await Task.WhenAll(gameLaunchHelperTask, supportedVersionsTask, downloadLinksTask);

        var downloadLinks = await downloadLinksTask;
        var gameLaunchHelper = await gameLaunchHelperTask;
        var supportedVersions = await supportedVersionsTask;

        List<VersionItem> versionItems = [];

        foreach (var item in downloadLinks["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var downloadVersion = item.Key[..index];

            GameVersion gameVersion = new(downloadVersion);
            var roundedVersion = RoundVersionBuild(gameVersion);

            if (!supportedVersions.Contains(roundedVersion))
                continue;

            versionItems.Add(new(downloadVersion, item.Value, gameLaunchHelper));
        }

        versionItems.Sort(s_comparer);

        return new VersionRegistry(supportedVersions, versionItems);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<VersionItem> GetEnumerator() => _versionItems.GetEnumerator();
}