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

public sealed class VersionCatalog
{
    VersionCatalog(ConcurrentDictionary<string, VersionEntry?> entries) => _entries = entries;

    [Obsolete("", true)]
    static readonly Comparer s_comparer = new();

    const string Uri = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly ConcurrentDictionary<string, VersionEntry?> _entries;

    public string SupportedVersion => _entries.Keys.First();

    public VersionEntry this[string version] => _entries[version] ?? throw new KeyNotFoundException();

    public IEnumerable<string> InstallableVersions => _entries.Keys;

    public bool IsSupported => _entries.ContainsKey(Minecraft.PackageVersion);

    static async Task<ConcurrentDictionary<string, VersionEntry?>> GetAsync()
    {
        ConcurrentDictionary<string, VersionEntry?> entires = [];

        using StreamReader reader = new(await HttpService.GetAsync<Stream>(Uri));
        string _; while ((_ = await reader.ReadLineAsync()) is { }) entires.TryAdd(_.Trim(), null);

        return entires;
    }

    [Obsolete("", true)]
    static async Task<SortedSet<string>> SupportedAsync()
    {
        string @string;
        SortedSet<string> supported = new(s_comparer);

        using StreamReader reader = new(await HttpService.GetAsync<Stream>(Uri));
        while ((@string = await reader.ReadLineAsync()) is { }) supported.Add(@string.Trim());

        return supported;
    }

    public static async Task<VersionCatalog> CreateAsync() => await Task.Run(async () =>
    {
        var entries = await GetAsync();
        await Task.WhenAll(UWPVersionEntry.GetAsync(entries), GDKVersionEntry.GetAsync(entries));
        return new VersionCatalog(entries);
    });

    [Obsolete("", true)]
    sealed class Comparer : IComparer<string>
    {
        public int Compare(string x, string y) => new Version(y).CompareTo(new Version(x));
    }
}