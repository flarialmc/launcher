using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;
using System.Linq;
using System.Collections.Concurrent;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class VersionCatalog : IEnumerable<KeyValuePair<string, VersionEntry?>>
{
    VersionCatalog(ConcurrentDictionary<string, VersionEntry?> entries) => _entries = entries;

    const string Uri = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly ConcurrentDictionary<string, VersionEntry?> _entries;

    public bool IsSupported => _entries.ContainsKey(Minecraft.PackageVersion);

    static async Task<ConcurrentDictionary<string, VersionEntry?>> GetAsync()
    {
        ConcurrentDictionary<string, VersionEntry?> entires = [];

        using StreamReader reader = new(await HttpService.GetAsync<Stream>(Uri));
        string _; while ((_ = await reader.ReadLineAsync()) is { }) entires.TryAdd(_.Trim(), null);

        return entires;
    }

    public static async Task<VersionCatalog> CreateAsync() => await Task.Run(async () =>
    {
        var entries = await GetAsync();
        await Task.WhenAll(UWPVersionEntry.GetAsync(entries), GDKVersionEntry.GetAsync(entries));
        return new VersionCatalog(entries);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionEntry?>> GetEnumerator() => _entries.GetEnumerator();

    [Obsolete("", true)]
    static async Task<SortedSet<string>> SupportedAsync()
    {
        string @string;
        SortedSet<string> supported = new(s_comparer);

        using StreamReader reader = new(await HttpService.GetAsync<Stream>(Uri));
        while ((@string = await reader.ReadLineAsync()) is { }) supported.Add(@string.Trim());

        return supported;
    }

    [Obsolete]
    public VersionEntry this[string version] => _entries[version] ?? throw new KeyNotFoundException();

    [Obsolete]
    public IEnumerable<string> InstallableVersions => _entries.Keys;

    [Obsolete]
    public string SupportedVersion => _entries.Keys.First();

    [Obsolete("", true)]
    static readonly Comparer s_comparer = new();

    [Obsolete("", true)]
    sealed class Comparer : IComparer<string>
    {
        public int Compare(string x, string y) => new Version(y).CompareTo(new Version(x));
    }
}