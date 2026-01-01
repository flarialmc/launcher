using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Services.Management.Versions;


sealed class GDKVersionEntry : VersionEntry
{
    const string PackagesUri = "https://raw.githubusercontent.com/MinecraftBedrockArchiver/GdkLinks/refs/heads/master/urls.json";

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), s_settings);

    readonly string[] _urls;

    GDKVersionEntry(string[] urls) : base() => _urls = urls;

    internal static async Task GetAsync(ConcurrentDictionary<string, VersionEntry?> entries) => await Task.Run(async () =>
    {
        using var stream = await HttpService.GetAsync<Stream>(PackagesUri);
        var items = (Dictionary<string, Dictionary<string, string[]>>)s_serializer.ReadObject(stream);

        foreach (var item in items["release"])
        {
            var key = item.Key.Substring(0, item.Key.LastIndexOf('.'));
            entries.TryUpdate(key, new GDKVersionEntry(item.Value), null);
        }
    });

    public override async Task<string> GetAsync() => _urls[0];
}