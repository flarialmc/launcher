using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Management.Versions;
using System.Linq;

namespace Flarial.Launcher.Services.SDK;

public sealed class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();
    static readonly AddPackageOptions Options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    readonly VersionCatalog _catalog;

    Catalog(VersionCatalog catalog) => _catalog = catalog;

    public static async Task<Catalog> GetAsync() => new(await VersionCatalog.GetAsync());

    public async Task<Uri> UriAsync(string version) => new(await _catalog[version].GetAsync());

    public bool IsCompatible => _catalog.IsSupported;

    public string LatestSupportedVersion => _catalog.LatestSupportedVersion;

    public async Task<Request> InstallAsync(string value, Action<int> action) => new(Manager.AddPackageByUriAsync(await UriAsync(value), Options), action);

    public IEnumerator<string> GetEnumerator() => _catalog.InstallableVersions.Reverse().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}