using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flarial.Launcher.Services.Networking;
using Windows.Data.Json;

namespace Flarial.Launcher.Services.SDK;

static class Web
{
    const string Packages = "https://cdn.jsdelivr.net/gh/ddf8196/mc-w10-versiondb-auto-update@refs/heads/master/versions.json.min";

    const string Supported = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    const string Store = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";

    internal static async Task<(HashSet<string> Supported, Dictionary<string, string> Packages)> VersionsAsync() => await Task.Run(async () =>
    {
        HashSet<string> supported = [];
        Dictionary<string, string> packages = [];

        using StreamReader stream = new(await HttpService.GetAsync<Stream>(Supported));

        string @string; while ((@string = stream.ReadLine()) != default)
            if (!string.IsNullOrEmpty(@string = @string.Trim()))
                supported.Add(@string);

        foreach (var item in JsonArray.Parse(await HttpService.GetAsync<string>(Packages)))
        {
            var array = item.GetArray(); if (array.GetNumberAt(2) != default) continue;
            var value = array.GetStringAt(default);

            if (!supported.Contains(value = value.Substring(default, value.LastIndexOf('.')))) continue;
            packages.Add(value, array.GetStringAt(1));
        }

        return (supported, packages);
    });

    internal static async Task<Uri> UriAsync(HttpContent content)
    {
        using var message = await HttpService.PostAsync(Store, content);
        message.EnsureSuccessStatusCode();

        using var stream = await message.Content.ReadAsStreamAsync();
        return new(XElement.Load(stream).Descendants().FirstOrDefault(_ => _.Value.StartsWith("http://tlu.dl.delivery.mp.microsoft.com", StringComparison.Ordinal)).Value);
    }
}