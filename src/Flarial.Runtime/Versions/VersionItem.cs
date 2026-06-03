using System;
using System.Collections.Generic;
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
    readonly string _string;
    public override string ToString() => _string;

    static readonly string s_temp = Path.GetTempPath();

    internal VersionItem(string version, string[] downloadUris, byte[] gamelaunchHelper)
    {
        _string = Stringify(version);
        _downloadUris = downloadUris;
        _gamelaunchHelper = gamelaunchHelper;
    }

    internal static string Stringify(string version)
    {
        VersionKey key = new(version);
        return key._minor >= 26 ? $"{key._minor}.{key._build}" : version;
    }

    readonly string[] _downloadUris;
    readonly byte[] _gamelaunchHelper;

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
        using CancellationTokenSource source = new();
        HashSet<Task<string?>> tasks = [.. _downloadUris.Select(_ => PingAsync(_, source.Token))];

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);

            if (await task is { } uri)
            {
                source.Cancel();
                return uri;
            }
        }

        throw new DownloadUriNotFoundException();
    }

    public async Task InstallAsync(Action<int, bool> callback)
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!Minecraft.IsInstalled)
            throw new MinecraftNotInstalledException();

        if (!Minecraft.IsPackaged)
            throw new MinecraftUnpackagedException();

        var package = Path.Combine(s_temp, Path.GetRandomFileName());
        var helper = Path.Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

        try
        {
            await HttpService.DownloadAsync(await GetAsync(), package, _ => callback(_, false));
            await PackageService.AddAsync(new(package), _ => callback(_, true));
            await File.WriteAllBytesAsync(helper, _gamelaunchHelper);
        }
        finally
        {
            try { File.Delete(package); }
            catch { }
        }
    }
}