using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Networking;
using Windows.Graphics;
using static System.IO.Path;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Services.Versions;

sealed class GDKVersionItem : VersionItem
{
    const string PackageFamilyName = "Microsoft.GamingServices_8wekyb3d8bbwe";

    const string GameLaunchHelperUrl = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";

    const string MSIXVCPackagesUrl = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@refs/heads/master/urls.json";

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), s_settings);

    readonly byte[] _bytes;
    readonly string[] _urls;

    static string Path => Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

    GDKVersionItem(string[] urls, byte[] bytes) => (_urls, _bytes) = (urls, bytes);

    internal static async Task QueryAsync(IDictionary<string, VersionRegistry.VersionEntry> registry) => await Task.Run(async () =>
    {
        var msixvcPackagesTask = HttpService.GetStreamAsync(MSIXVCPackagesUrl);
        var gameLaunchHelperTask = HttpService.GetBytesAsync(GameLaunchHelperUrl);
        await Task.WhenAll(msixvcPackagesTask, gameLaunchHelperTask);

        var gameLaunchHelper = await gameLaunchHelperTask;
        using var msixvcPackages = await msixvcPackagesTask;

        Dictionary<string, Dictionary<string, string[]>> items; unsafe
        {
            var @object = s_serializer.ReadObject(msixvcPackages);
            items = *(Dictionary<string, Dictionary<string, string[]>>*)&@object;
        }

        foreach (var item in items["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var key = item.Key.Substring(0, index);

            lock (registry)
            {
                GDKVersionItem value = new(item.Value, gameLaunchHelper);
                if (registry.TryGetValue(key, out var entry)) entry._item = value;
                else registry.Add(key, new(false) { _item = value });
            }
        }
    });

    static async Task<string?> PingAsync(string url, CancellationToken token)
    {
        try
        {
            using var message = await HttpService.GetAsync(url, token);
            return message.IsSuccessStatusCode ? url : null;
        }
        catch { return null; }
    }

    public override async Task<string> GetAsync()
    {
        using CancellationTokenSource source = new();
        HashSet<Task<string?>> tasks = [.. _urls.Select(_ => PingAsync(_, source.Token))];

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks); tasks.Remove(task);
            if (await task is { } url) { source.Cancel(); return url; }
        }

        throw new InvalidOperationException();
    }

    public override async Task InstallAsync(Action<int, bool> action)
    {
        /*
           - Verify, if GRTS is available and installed.
           - If available, proceed with downloading & installing the package.
        */

        if (!Minecraft.GamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

        await base.InstallAsync(action);

        /*
           - Replace the stock PC Bootstrapper with a custom one.
           - This suppresses auto-updates allowing the version switch to persist. 
        */

        using var stream = File.Create(Path);
        await stream.WriteAsync(_bytes, 0, _bytes.Length);
    }
}