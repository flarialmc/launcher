using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flarial.Launcher.Services.Networking;
using Microsoft.PowerShell;

namespace Flarial.Launcher.Services.Management.Versions;

sealed class UWPVersionEntry : VersionEntry
{
    const string MediaType = "application/soap+xml";
    const string DownloadUri = "http://tlu.dl.delivery.mp.microsoft.com";
    const string StoreUri = "https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";
    const string PackagesUri = "https://cdn.jsdelivr.net/gh/ddf8196/mc-w10-versiondb-auto-update@refs/heads/master/versions.json.min";

    readonly string _content;
    static readonly string s_content;
    static readonly DataContractJsonSerializer s_serializer = new(typeof(string[][]), s_settings);

    UWPVersionEntry(string identifier) : base() => _content = string.Format(s_content, identifier, '1');

    static UWPVersionEntry()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream("GetExtendedUpdateInfo2.xml");
        using StreamReader reader = new(stream);

        s_content = reader.ReadToEnd();
    }

    internal static async Task CreateAsync(ConcurrentDictionary<string, VersionEntry?> entries)
    {
        using var stream = await HttpService.StreamAsync(PackagesUri);
        var items = (string[][])s_serializer.ReadObject(stream);

        foreach (var item in items)
        {
            if (item[2] != "0") continue;
            var key = item[0].Substring(0, item[0].LastIndexOf('.'));
            entries.TryUpdate(key, new UWPVersionEntry(item[1]), null);
        }
    }

    public override async Task<string> GetAsync() => await Task.Run(async () =>
    {
        using StringContent content = new(_content, Encoding.UTF8, MediaType);
        using var message = await HttpService.PostAsync(StoreUri, content);

        message.EnsureSuccessStatusCode();
        using var stream = await message.Content.ReadAsStreamAsync();

        var descendants = XElement.Load(stream).Descendants();
        return descendants.First(_ => _.Value.StartsWith(DownloadUri, StringComparison.OrdinalIgnoreCase)).Value;
    });
}