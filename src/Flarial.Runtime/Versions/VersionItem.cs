using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public sealed class VersionItem
{

    public override string ToString() => _string;

    static readonly string s_temp = Path.GetTempPath();

    internal VersionItem(string version, string[] downloadUris, byte[] gameLaunchHelper)
    {
        _string = Stringify(version);
        _downloadUris = downloadUris;
        _gameLaunchHelper = gameLaunchHelper;
    }

    internal static string Stringify(string version)
    {
        VersionKey key = new(version);
        return key._minor >= 26 ? $"{key._minor}.{key._build}" : version;
    }

    readonly string _string;
    readonly string[] _downloadUris;
    readonly byte[] _gameLaunchHelper;

    static async Task<string?> PingAsync(string uri, CancellationToken token)
    {
        try
        {
            using var response = await HttpService.GetAsync(uri, token);
            return response.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }

    async Task<string> GetAsync()
    {
        using CancellationTokenSource cts = new();
        var tasks = _downloadUris.Select(_ => PingAsync(_, cts.Token));

        await foreach (var task in Task.WhenEach(tasks)) if (await task is { } uri)
        {
            cts.Cancel();
            return uri;
        }

        throw new DownloadLinksNotFoundException();
    }

    public async Task InstallAsync(Action<int, bool> callback)
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!Minecraft.IsInstalled)
            throw new GameNotInstalledException();

        if (Minecraft.IsSideloaded)
            throw new GameSideloadedException();

        var package = Path.Combine(s_temp, Path.GetRandomFileName());
        var helper = Path.Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

        try
        {
            await HttpService.DownloadAsync(await GetAsync(), package, _ => callback(_, false));
            await PackageService.AddAsync(new(package), _ => callback(_, true));
            await File.WriteAllBytesAsync(helper, _gameLaunchHelper);
        }
        finally
        {
            try { File.Delete(package); }
            catch { }
        }
    }
}