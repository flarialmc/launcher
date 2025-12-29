using System;
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

    internal static async Task<Dictionary<string, VersionEntry>> GetAsync(SortedSet<string> supported) => await Task.Run(async () =>
    {
        string[][] collection;
        Dictionary<string, VersionEntry> entries = [];

        using (var stream = await HttpService.GetAsync<Stream>(PackagesUri))
        {
            var @object = s_serializer.ReadObject(stream);
            collection = (string[][])@object;
        }

        foreach (var item in collection)
        {
            if (item[2] != "0") continue;

            var version = item[0];
            var index = version.LastIndexOf('.');

            version = version.Substring(0, index);
            if (!supported.Contains(version)) continue;
            entries.Add(version, new UWPVersionEntry(item[1]));
        }

        return entries;
    });

    internal override async Task<string> GetAsync() => await Task.Run(async () =>
    {
        using StringContent content = new(_content, Encoding.UTF8, MediaType);
        using var message = await HttpService.PostAsync(StoreUri, content);

        message.EnsureSuccessStatusCode();
        using var stream = await message.Content.ReadAsStreamAsync();

        var descendants = XElement.Load(stream).Descendants();
        return descendants.First(_ => _.Value.StartsWith(DownloadUri, StringComparison.OrdinalIgnoreCase)).Value;
    });
}