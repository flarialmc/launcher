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

public sealed class VersionRegistry : IEnumerable<KeyValuePair<string, VersionItem?>>
{
    VersionRegistry(ConcurrentDictionary<string, VersionItem?> registry) => _registry = registry;

    const string SupportedVersionsUrl = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly ConcurrentDictionary<string, VersionItem?> _registry;

    public bool IsSupported => _registry.ContainsKey(Minecraft.PackageVersion);

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        ConcurrentDictionary<string, VersionItem?> registry = [];
        using var stream = await HttpStack.GetStreamAsync(SupportedVersionsUrl);

        using StreamReader reader = new(stream);
        string _; while ((_ = await reader.ReadLineAsync()) is { }) registry.TryAdd(_.Trim(), null);

        var uwp = UWPVersionItem.CreateAsync(registry);
        var gdk = GDKVersionItem.CreateAsync(registry);
        await Task.WhenAll(uwp, gdk);

        return new VersionRegistry(registry);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionItem?>> GetEnumerator() => _registry.OrderByDescending(static _ => new Version(_.Key)).GetEnumerator();
}