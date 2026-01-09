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

    public static async Task<VersionEntries> CreateAsync() => await Task.Run(static async () =>
    {
        ConcurrentDictionary<string, VersionEntry?> entries = [];
using var stream = await HttpService.StreamAsync(Uri);

        using StreamReader reader = new(stream);
        string _; while ((_ = await reader.ReadLineAsync()) is { }) entries.TryAdd(_.Trim(), null);

        var uwp = UWPVersionEntry.CreateAsync(entries);
        var gdk = GDKVersionEntry.CreateAsync(entries);
        await Task.WhenAll(uwp, gdk);

        return new VersionEntries(entries);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionEntry?>> GetEnumerator() => _entries.OrderByDescending(static _ => new Version(_.Key)).GetEnumerator();
}