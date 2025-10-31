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

namespace Flarial.Launcher.SDK;

public sealed class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();

    readonly HashSet<string> Supported;

    readonly Dictionary<string, string> Packages;

    static readonly string Content = new Func<string>(() =>
    {
        using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("GetExtendedUpdateInfo2.xml"));
        return reader.ReadToEnd();
    })();

    static readonly AddPackageOptions Options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    Catalog(HashSet<string> supported, Dictionary<string, string> packages) => (Supported, Packages) = (supported, packages);

    public static async Task<Catalog> GetAsync()
    {
        var _ = await Web.VersionsAsync();
        return new(_.Supported, _.Packages);
    }

    public async Task<Uri> UriAsync(string value) => await Task.Run(async () =>
    {
        using StringContent content = new(string.Format(Content, Packages[value], '1'), Encoding.UTF8, "application/soap+xml");
        return await Web.UriAsync(content);
    });

    public async Task<bool> CompatibleAsync() => await Task.Run(() => Supported.Contains(Minecraft.Version));

    public async Task<Request> InstallAsync(string value, Action<int> action) => new(Manager.AddPackageByUriAsync(await UriAsync(value), Options), action);

    public IEnumerator<string> GetEnumerator() => Packages.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}