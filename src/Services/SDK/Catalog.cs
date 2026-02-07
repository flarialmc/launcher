using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;
using Flarial.Launcher.Services.Versions;
using System.Linq;
using Flarial.Launcher.Services.Game;

namespace Flarial.Launcher.Services.SDK;

public sealed class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();

    static readonly AddPackageOptions Options = new()
    {
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };

    readonly Dictionary<string, VersionItem> _catalog;

    readonly VersionRegistry _registry;

    Catalog(VersionRegistry registry) => _catalog = (_registry = registry).ToDictionary(static _ => _.Key, static _ => _.Value);

    public static async Task<Catalog> GetAsync() => new(await VersionRegistry.CreateAsync());

    public async Task<Uri> UriAsync(string version) => new(await _catalog[version].GetAsync());

    public bool IsCompatible => _registry.Supported;

    public async Task<Request> InstallAsync(string value, Action<int> action) => new(Manager.AddPackageByUriAsync(await UriAsync(value), Options), action);

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var item in _catalog)
            yield return item.Key;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}