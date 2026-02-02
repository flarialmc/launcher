using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Services.Versions;

sealed class UWPVersionItem : VersionItem
{
    const string AppxPackageDownloadUrl = "http://tlu.dl.delivery.mp.microsoft.com";

    const string MicrosoftStoreUrl = "https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";

    const string AppxPackagesUrl = "https://cdn.jsdelivr.net/gh/ddf8196/mc-w10-versiondb-auto-update@refs/heads/master/versions.json.min";

    readonly string _content; 
    static readonly string s_content;
    static readonly DataContractJsonSerializer s_serializer = new(typeof(string[][]), s_settings);

    UWPVersionItem(string identifier) => _content = string.Format(s_content, identifier, '1');

    static UWPVersionItem()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream("GetExtendedUpdateInfo2.xml");
        using StreamReader reader = new(stream);

        s_content = reader.ReadToEnd();
    }

    internal static async Task QueryAsync(Dictionary<string, VersionItem?> registry) => await Task.Run(async () =>
    {
        using var stream = await HttpService.GetStreamAsync(AppxPackagesUrl);
        var items = (string[][])s_serializer.ReadObject(stream);

        foreach (var item in items)
        {
            if (item[2] != "0") continue;
            var key = item[0].Substring(0, item[0].LastIndexOf('.'));

            lock (registry)
            {
                if (!registry.TryGetValue(key, out _)) continue;
                registry[key] = new UWPVersionItem(item[1]);
            }
        }
    });

    public override async Task<string> GetAsync() => await Task.Run(async () =>
    {
        using StringContent content = new(_content, Encoding.UTF8, "application/soap+xml");
        using var message = await HttpService.PostAsync(MicrosoftStoreUrl, content);

        message.EnsureSuccessStatusCode();
        using var stream = await message.Content.ReadAsStreamAsync();

        var descendants = XElement.Load(stream).Descendants();
        return descendants.First(_ => _.Value.StartsWith(AppxPackageDownloadUrl, StringComparison.OrdinalIgnoreCase)).Value;
    });
}