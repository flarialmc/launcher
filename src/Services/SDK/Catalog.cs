using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;
using Flarial.Launcher.Services.Management.Versions;
using System.Linq;
using Flarial.Launcher.Services.Core;

namespace Flarial.Launcher.Services.SDK;

public sealed class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();

    static readonly AddPackageOptions Options = new()
    {
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };

    readonly Dictionary<string, VersionEntry> _entries;

    Catalog(VersionEntries entries) => _entries = entries.ToDictionary(static _ => _.Key, static _ => _.Value);

    public static async Task<Catalog> GetAsync() => new(await VersionEntries.CreateAsync());

    public async Task<Uri> UriAsync(string version) => new(await _entries[version].GetAsync());

    public bool IsCompatible => _entries.ContainsKey(Minecraft.PackageVersion);

    public async Task<Request> InstallAsync(string value, Action<int> action) => new(Manager.AddPackageByUriAsync(await UriAsync(value), Options), action);

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var entry in _entries)
        {
            if (entry.Value is null) continue;
            yield return entry.Key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}