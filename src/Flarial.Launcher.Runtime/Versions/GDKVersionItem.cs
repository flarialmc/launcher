using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Services;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Runtime.Versions;

sealed class GDKVersionItem : VersionItem
{
    const string GameLaunchHelperUri = "https://cdn.flarial.xyz/launcher/gamelaunchhelper.dll";
    const string MSIXVCPackagesUri = "https://cdn.jsdelivr.net/gh/MinecraftBedrockArchiver/GdkLinks@latest/urls.json";

    static readonly JsonService<Dictionary<string, Dictionary<string, string[]>>> s_json;
    static GDKVersionItem() => s_json = JsonService<Dictionary<string, Dictionary<string, string[]>>>.GetJson();

    readonly string[] _uris;
    readonly byte[] _gameLaunchHelper;

    GDKVersionItem(string version, string[] uris, byte[] gameLaunchHelper) : base(version) => (_uris, _gameLaunchHelper) = (uris, gameLaunchHelper);

    internal static async Task QueryAsync(SortedDictionary<string, VersionRegistry.VersionEntry> registry) => await Task.Run(async () =>
    {
        var msixvcPackagesTask = HttpService.GetStreamAsync(MSIXVCPackagesUri);
        var gameLaunchHelperTask = HttpService.GetBytesAsync(GameLaunchHelperUri);
        await Task.WhenAll(msixvcPackagesTask, gameLaunchHelperTask);

        var gameLaunchHelper = await gameLaunchHelperTask;
        using var msixvcPackages = await msixvcPackagesTask;

        foreach (var item in s_json.ReadStream(msixvcPackages)["release"])
        {
            var index = item.Key.LastIndexOf('.');
            var key = item.Key.Substring(0, index);

            lock (registry)
            {
                if (!registry.TryGetValue(key, out var entry))
                    continue;

                var version = VersionRegistry.NormalizeVersion(key);
                entry._item = new GDKVersionItem(version, item.Value, gameLaunchHelper);
            }
        }
    });

    static async Task<string?> PingAsync(string uri, CancellationToken token)
    {
        try
        {
            using var message = await HttpService.GetAsync(uri, token);
            return message.IsSuccessStatusCode ? uri : null;
        }
        catch { return null; }
    }

    protected override async Task<string> GetUriAsync()
    {
        using CancellationTokenSource source = new();
        HashSet<Task<string?>> tasks = [.. _uris.Select(_ => PingAsync(_, source.Token))];

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

    public override async Task InstallAsync(Action<int, bool> action)
    {
        if (!Minecraft.IsGamingServicesInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PREREQUISITE_FAILED);

        await base.InstallAsync(action);
        var path = Path.Combine(Minecraft.Package.InstalledPath, "gamelaunchhelper.dll");

        using var stream = File.Create(path);
        await stream.WriteAsync(_gameLaunchHelper, 0, _gameLaunchHelper.Length);
    }
}