using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Net.Http;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;
using System.IO.Compression;
using Bedrockix.Minecraft;
using System.Threading;

namespace Flarial.Launcher.SDK;

public sealed partial class Catalog : IEnumerable<string>
{
    static readonly PackageManager Manager = new();

    static readonly SemaphoreSlim Semaphore = new(1, 1);

    readonly HashSet<string> Supported;

    readonly Dictionary<string, string> Packages;

    static readonly string Content = new Func<string>(() =>
    {
        using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("GetExtendedUpdateInfo2.xml"));
        return reader.ReadToEnd();
    })();

    static readonly AddPackageOptions Options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    Catalog(HashSet<string> supported, Dictionary<string, string> packages) => (Supported, Packages) = (supported, packages);

    public static async partial Task<Catalog> GetAsync()
    {
        var _ = await Web.VersionsAsync();
        return new(_.Supported, _.Packages);
    }

    public static partial async Task FrameworksAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            if (Manager.FindPackagesForUser(string.Empty, "Microsoft.Services.Store.Engagement_8wekyb3d8bbwe").Any()) return;

            await Task.Run(async () =>
            {
                var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                using (var stream = await Web.FrameworksAsync()) using (ZipArchive archive = new(stream))
                    archive.Entries.First(_ => _.Name is "Microsoft.Services.Store.Engagement.x64.10.0.appx").ExtractToFile(path, true);

                await Manager.AddPackageByUriAsync(new(path), Options);
            });
        }
        finally { Semaphore.Release(); }
    }

    public partial async Task<Uri> UriAsync(string value) => await Task.Run(async () =>
    {
        using StringContent content = new(string.Format(Content, Packages[value], '1'), Encoding.UTF8, "application/soap+xml");
        await FrameworksAsync(); return await Web.UriAsync(content);
    });

    public async partial Task<bool> CompatibleAsync() => await Task.Run(() => Supported.Contains(Metadata.Version));

    public async partial Task<Request> InstallAsync(string value, Action<int> action) => new(Manager.AddPackageByUriAsync(await UriAsync(value), Options), action);

    /// <summary>
    /// Enumerates versions present in the catalog.
    /// </summary>

    public IEnumerator<string> GetEnumerator() => Packages.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}