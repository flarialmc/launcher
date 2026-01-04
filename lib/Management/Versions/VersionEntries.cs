using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;
using System.Collections.Concurrent;
using System.Linq;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class VersionEntries : IEnumerable<KeyValuePair<string, VersionEntry?>>
{
    VersionEntries(ConcurrentDictionary<string, VersionEntry?> entries) => _entries = entries;

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

    public static async Task<VersionEntries> CreateAsync() => await Task.Run(static async () =>
    {
        var entries = await GetAsync();
        await Task.WhenAll(UWPVersionEntry.GetAsync(entries), GDKVersionEntry.GetAsync(entries));
        return new VersionEntries(entries);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionEntry?>> GetEnumerator() => _entries.OrderByDescending(static _ => new Version(_.Key)).GetEnumerator();
}