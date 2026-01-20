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

sealed class GDKVersionEntry : VersionEntry
{
    const string PackageFamilyName = "Microsoft.GamingServices_8wekyb3d8bbwe";

    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";

    const string PackagesUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@refs/heads/master/urls.json";

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), s_settings);

    readonly byte[] _bytes;
    readonly string[] _uris;

    static string Path => Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

    GDKVersionEntry(string[] uris, byte[] bytes) => (_uris, _bytes) = (uris, bytes);

    internal static async Task CreateAsync(ConcurrentDictionary<string, VersionEntry?> entries) => await Task.Run(async () =>
    {
        var streamTask = HttpService.StreamAsync(PackagesUri);
        var bytesTask = HttpService.BytesAsync(GameLaunchHelperUri);
        await Task.WhenAll(streamTask, bytesTask);

        var bytes = await bytesTask;
        using var stream = await streamTask;

        var items = (Dictionary<string, Dictionary<string, string[]>>)s_serializer.ReadObject(stream);

        foreach (var item in items["release"])
        {
            var key = item.Key.Substring(0, item.Key.LastIndexOf('.'));
            entries.TryUpdate(key, new GDKVersionEntry(item.Value, bytes), null);
        }
    });

    static async Task<string?> GetAsync(string uri, CancellationToken token)
    {
        try
        {
            using var message = await HttpService.GetAsync(uri, token);
            return message.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }

    public override async Task<string> GetAsync()
    {
        using CancellationTokenSource source = new();
        HashSet<Task<string?>> tasks = [.. _uris.Select(_ => GetAsync(_, source.Token))];

        while (tasks.Count > 0)
        {
            var task = await Task.WhenAny(tasks); tasks.Remove(task);
            if (await task is { } uri) { source.Cancel(); return uri; }
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