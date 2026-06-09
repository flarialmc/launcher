using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using Windows.System.Profile;

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
        HashSet<Task<string?>> tasks = [.. _downloadUris.Select(_ => PingAsync(_, cts.Token))];

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);

            if (await task is { } uri)
            {
                cts.Cancel();
                return uri;
            }
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

        var path = Path.Combine(s_temp, Path.GetRandomFileName());
        try
        {
            await HttpService.DownloadAsync(await GetAsync(), path, _ => callback(_, false));
            await PackageService.AddAsync(new(path), _ => callback(_, true));

            var installedPath = Minecraft.Package.InstalledPath;
            var gameLaunchHelperPath = Path.Combine(installedPath, "gamelaunchhelper.dll");

            using var stream = File.Create(gameLaunchHelperPath);
            await stream.WriteAsync(_gameLaunchHelper, 0, _gameLaunchHelper.Length);
        }
        finally
        {
            try { File.Delete(path); }
            catch { }
        }
    }
}