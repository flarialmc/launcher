using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Win32.Foundation;
using static System.IO.Path;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Services.Management.Versions;

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

    internal static async Task CreateAsync(ConcurrentDictionary<string, VersionItem?> registry) => await Task.Run(async () =>
    {
        var msixvcPackagesTask = HttpStack.GetStreamAsync(MSIXVCPackagesUrl);
        var gamelaunchHelperTask = HttpStack.GetBytesAsync(GameLaunchHelperUrl);
        await Task.WhenAll(msixvcPackagesTask, gamelaunchHelperTask);

        var bytes = await gamelaunchHelperTask;
        using var stream = await msixvcPackagesTask;

        var items = (Dictionary<string, Dictionary<string, string[]>>)s_serializer.ReadObject(stream);

        foreach (var item in items["release"])
        {
            var key = item.Key.Substring(0, item.Key.LastIndexOf('.'));
            registry.TryUpdate(key, new GDKVersionItem(item.Value, bytes), null);
        }
    });

    static async Task<string?> PingAsync(string url, CancellationToken token)
    {
        try
        {
            using var message = await HttpStack.GetAsync(url, token);
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

        if (!s_manager.FindPackagesForUser(string.Empty, PackageFamilyName).Any())
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