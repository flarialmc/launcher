using System;
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

    internal VersionItem(string version, string[] downloadUris, byte[] gameLaunchHelper)
    {
        _string = Stringify(version);
        _downloadUris = downloadUris;
        _gameLaunchHelper = gameLaunchHelper;
    }

    internal static string Stringify(string version)
    {
        GameVersion value = new(version);

        if (value._minor >= 26)
            return $"{value._minor}.{value._build}";

        return version;
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

    async Task<string?> GetAsync()
    {
        using CancellationTokenSource cts = new();
        var tasks = _downloadUris.Select(_ => PingAsync(_, cts.Token));

        await foreach (var task in Task.WhenEach(tasks))
        {
            var uri = await task;
            if (uri is null) continue;

            cts.Cancel();
            return uri;
        }

        return null;
    }

    public async Task<InstallRequest?> InstallAsync()
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!Minecraft.IsInstalled)
            throw new MinecraftNotInstalledException();

        if (Minecraft.IsSideloaded)
            throw new MinecraftSideloadedException();

        if (await GetAsync() is not { } downloadUri)
            return null;

        return new(downloadUri, _gameLaunchHelper);
    }
}