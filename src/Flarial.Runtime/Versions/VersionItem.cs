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
    static readonly string s_path = Path.GetTempPath();

    public override string ToString() => _string;

    internal VersionItem(string version, string[] downloadUris, byte[] gameLaunchHelper)
    {
        _downloadUris = downloadUris;
        _gameLaunchHelper = gameLaunchHelper;
        _string = new GameVersion(version).ToString();
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

    async Task<string?> GetUriAsync()
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

    async Task InstallAsync(string uri, Action<int, bool> callback)
    {
        var packagePath = Path.Combine(s_path, Path.GetRandomFileName());

        try
        {
            await HttpService.DownloadAsync(uri, packagePath, OnDownload);
            await Task.Run(() => PackageService.Add(packagePath, OnInstall));

            var installedPath = Minecraft.Package.InstalledPath;
            var gameLaunchHelperPath = Path.Combine(installedPath, "gamelaunchhelper.dll");

            await File.WriteAllBytesAsync(gameLaunchHelperPath, _gameLaunchHelper);
        }
        finally
        {
            try { File.Delete(packagePath); }
            catch { }
        }

        void OnInstall(int value) => callback(value, true);
        void OnDownload(int value) => callback(value, false);
    }

    public async Task<Task?> InstallAsync(Action<int, bool> callback)
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!Minecraft.IsInstalled)
            throw new MinecraftNotInstalledException();

        if (Minecraft.IsSideloaded)
            throw new MinecraftSideloadedException();

        if (await GetUriAsync() is not { } uri)
            return null;

        return InstallAsync(uri, callback);
    }
}