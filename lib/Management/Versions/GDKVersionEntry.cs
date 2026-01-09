using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Networking.Vpn;
using static System.IO.Path;

namespace Flarial.Launcher.Services.Management.Versions;

sealed class GDKVersionEntry : VersionEntry
{
    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";

    const string PackagesUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@refs/heads/master/urls.json";

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), s_settings);

    readonly byte[] _array;
    readonly string[] _uris;

    static string Path => Combine(Minecraft.Package.InstalledPath, "GameLaunchHelper.dll");

    GDKVersionEntry(string[] uris, byte[] array) => (_uris, _array) = (uris, array);

    internal static async Task CreateAsync(ConcurrentDictionary<string, VersionEntry?> entries) => await Task.Run(async () =>
    {
        var task1 = HttpService.StreamAsync(PackagesUri);
        var task2 = HttpService.BytesAsync(GameLaunchHelperUri);
        await Task.WhenAll(task1, task2);

        using var stream = await task1; var array = await task2;
        var items = (Dictionary<string, Dictionary<string, string[]>>)s_serializer.ReadObject(stream);

        foreach (var item in items["release"])
        {
            var key = item.Key.Substring(0, item.Key.LastIndexOf('.'));
            entries.TryUpdate(key, new GDKVersionEntry(item.Value, array), null);
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

    public override async Task InstallAsync(Action<AppInstallState, int> action)
    {
        await base.InstallAsync(action);

        /*
           - Replace the stock PC Bootstrapper with a custom one.
           - This suppresses auto-updates allowing the version switch to persist. 
        */

        using var stream = File.Create(Path);
        await stream.WriteAsync(_array, 0, _array.Length);
    }
}