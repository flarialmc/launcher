using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;
using Flarial.Launcher.Services.Management.Versions;

namespace Flarial.Launcher.Services.SDK;

public sealed class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();
    static readonly AddPackageOptions Options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    readonly VersionEntries _entries;

    Catalog(VersionEntries catalog) => _entries = catalog;

    public static async Task<Catalog> GetAsync() => new(await VersionEntries.CreateAsync());

#pragma warning disable CS0612

    /*
        - Allow the legacy VersionCatalog to directly access the underlying dictionary of the new VersionCatalog.
    */

    public async Task<Uri> UriAsync(string version) => new(await _entries[version].GetAsync());

#pragma warning restore CS0612

    public bool IsCompatible => _entries.IsSupported;

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