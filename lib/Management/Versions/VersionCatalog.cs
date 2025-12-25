using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class VersionCatalog
{
    VersionCatalog(HashSet<string> supported, SortedDictionary<string, VersionEntry> entries) => (_supported, _entries) = (supported, entries);

    static readonly Comparer s_comparer = new();
    const string Uri = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly HashSet<string> _supported;
    readonly SortedDictionary<string, VersionEntry> _entries;

    public VersionEntry this[string version] => _entries[version];
    public IEnumerable<string> InstallableVersions => _entries.Keys;
    public bool IsSupported => _supported.Contains(Minecraft.Version);

    static async Task<HashSet<string>> SupportedAsync()
    {
        string @string;
        HashSet<string> supported = [];

        using StreamReader reader = new(await HttpService.GetAsync<Stream>(Uri));
        while ((@string = await reader.ReadLineAsync()) is { }) supported.Add(@string.Trim());

        return supported;
    }

    public static async Task<VersionCatalog> GetAsync()
    {
        var supported = await SupportedAsync();

        var tasks = new Task<Dictionary<string, VersionEntry>>[2];
        tasks[0] = UWPVersionEntry.GetAsync(supported);
        tasks[1] = GDKVersionEntry.GetAsync(supported);
        await Task.WhenAll(tasks);

        SortedDictionary<string, VersionEntry> entries = new(s_comparer);
        foreach (var item in await tasks[0]) entries.Add(item.Key, item.Value);
        foreach (var item in await tasks[1]) entries.Add(item.Key, item.Value);

        return new(supported, entries);
    }

    sealed class Comparer : IComparer<string>
    {
        public int Compare(string x, string y) => new Version(y).CompareTo(new Version(x));
    }
}