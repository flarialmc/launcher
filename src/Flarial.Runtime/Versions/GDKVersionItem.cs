using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Runtime.Versions;

sealed class GDKVersionItem : VersionItem
{
    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";
    const string MSIXVCPackagesUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";

    readonly string[] _downloadUris;
    readonly byte[] _gameLaunchHelper;

    GDKVersionItem(string version, string[] downloadUris, byte[] gameLaunchHelper) : base(version)
    {
        _downloadUris = downloadUris;
        _gameLaunchHelper = gameLaunchHelper;
    }

    internal static async Task QueryAsync(SortedDictionary<string, VersionRegistry.VersionEntry> registry)
    {
        var msixvcPackagesTask = HttpStack.GetStreamAsync(MSIXVCPackagesUri);
        var gameLaunchHelperTask = HttpStack.GetBytesAsync(GameLaunchHelperUri);
       
        await Task.WhenAll(msixvcPackagesTask, gameLaunchHelperTask);
        var gameLaunchHelper = await gameLaunchHelperTask;
       
        using var stream = await msixvcPackagesTask;
        var json = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, string[]>>>(stream);

        foreach (var item in json["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var key = item.Key.Substring(0, index);

            if (!registry.TryGetValue(key, out var entry)) continue;
            entry._item = new GDKVersionItem(key, item.Value, gameLaunchHelper);
        }
    }

    static async Task<string?> PingAsync(string uri, CancellationToken token)
    {
        try
        {
            using var message = await HttpStack.GetAsync(uri, token);
            return message.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }

    protected override async Task<string> GetUriAsync()
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

        throw new InvalidOperationException();
    }

    public override async Task InstallAsync(Action<int, bool> callback)
    {
        if (!Minecraft.IsGamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

        await base.InstallAsync(callback);
        var path = Path.Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

        using var stream = File.Create(path);
        await stream.WriteAsync(_gameLaunchHelper, 0, _gameLaunchHelper.Length);
    }
}