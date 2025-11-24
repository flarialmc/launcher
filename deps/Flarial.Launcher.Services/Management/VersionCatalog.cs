using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Services.Management;

public sealed partial class VersionCatalog
{
    public IEnumerable<string> Versions => _catalog.Concat(_versions.Keys).Reverse();

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });

    const string Uri = "https://raw.githubusercontent.com/MinecraftBedrockArchiver/GdkLinks/refs/heads/master/urls.json";

    readonly SDK.Catalog _catalog;

    readonly Dictionary<string, string[]> _versions;

    VersionCatalog(SDK.Catalog catalog, Dictionary<string, string[]> versions)
    {
        _versions = versions;
        _catalog = catalog;
    }

    public static async Task<VersionCatalog> GetAsync()
    {
        var task1 = SDK.Catalog.GetAsync();
        var task2 = Task.Run(async () =>
        {
            Dictionary<string, string[]> versions = [];
            using var stream = await HttpService.GetAsync<Stream>(Uri);

            foreach (var item in (Dictionary<string, string[]>)((IDictionary)s_serializer.ReadObject(stream))["release"])
                versions[item.Key.Substring(0, item.Key.LastIndexOf('.'))] = item.Value;

            return versions;
        });

        await Task.WhenAll(task1, task2);
        return new(await task1, await task2);
    }
}