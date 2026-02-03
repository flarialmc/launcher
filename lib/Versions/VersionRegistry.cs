using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Networking;

namespace Flarial.Launcher.Services.Versions;

public sealed class VersionEntry
{
    public VersionItem? Item { get; internal set => field ??= value; }
}

public sealed class VersionRegistry : IEnumerable<KeyValuePair<string, VersionEntry>>
{
    static readonly VersionRegistryKeyComparer s_comparer = new();

    const string SupportedVersionsUrl = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly IReadOnlyDictionary<string, VersionEntry> _registry;

    VersionRegistry(IReadOnlyDictionary<string, VersionEntry> registry) => _registry = registry;

    public bool IsSupported => _registry.ContainsKey(Minecraft.PackageVersion);

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        SortedDictionary<string, VersionEntry> registry = new(s_comparer);
        using var stream = await HttpService.GetStreamAsync(SupportedVersionsUrl);

        string @string = string.Empty;
        using StreamReader reader = new(stream);

        while ((@string = await reader.ReadLineAsync()) is { })
            registry.Add(@string.Trim(), new());

        var uwp = UWPVersionItem.QueryAsync(registry);
        var gdk = GDKVersionItem.QueryAsync(registry);
        await Task.WhenAll(uwp, gdk);

        return new VersionRegistry(registry);
    });

    /*
        - This might be a "micro-optimization".
        - We can avoid using `System.Version` to avoid potential overhead.
    */

    sealed class VersionRegistryKeyComparer : IComparer<string>
    {
        unsafe readonly struct VersionRegistryKey
        {
            internal VersionRegistryKey(string version)
            {
                sbyte index = 0;
                var segments = stackalloc ushort[3];

                foreach (var value in version)
                {
                    if (value is '.')
                    {
                        ++index;
                        continue;
                    }

                    var segment = value - '0';
                    segment += segments[index] * 10;
                    segments[index] = (ushort)segment;
                }

                _major = segments[0];
                _minor = segments[1];
                _build = segments[2];
            }

            internal readonly ushort _major, _minor, _build;
        }

        public int Compare(string x, string y)
        {
            VersionRegistryKey a = new(x), b = new(y);

            if (b._major != a._major)
                return b._major.CompareTo(a._major);

            if (b._minor != a._minor)
                return b._minor.CompareTo(a._minor);

            return b._build.CompareTo(a._build);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionEntry>> GetEnumerator() => _registry.GetEnumerator();
}