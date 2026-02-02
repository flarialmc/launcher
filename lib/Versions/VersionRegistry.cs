using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Networking;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Windows.Networking.Vpn;
using System.Diagnostics;

namespace Flarial.Launcher.Services.Versions;

public sealed class VersionRegistry : IEnumerable<KeyValuePair<string, VersionItem?>>
{
    VersionRegistry(Dictionary<string, VersionItem?> registry) => _registry = registry;

    const string SupportedVersionsUrl = "https://cdn.flarial.xyz/launcher/NewSupported.txt";

    readonly Dictionary<string, VersionItem?> _registry;

    public bool IsSupported => _registry.ContainsKey(Minecraft.PackageVersion);

    /*
        - As the version metadata grows so does the processing time.
        - Use dedicated thread pools to improve frontend responsiveness & performance.
    */

    public static async Task<VersionRegistry> CreateAsync() => await Task.Run(static async () =>
    {
        Dictionary<string, VersionItem?> registry = [];
        using var stream = await HttpService.GetStreamAsync(SupportedVersionsUrl);

        string @string = string.Empty;
        using StreamReader reader = new(stream);

        while ((@string = await reader.ReadLineAsync()) is { })
            registry[@string.Trim()] = null;

        var uwp = UWPVersionItem.QueryAsync(registry);
        var gdk = GDKVersionItem.QueryAsync(registry);
        await Task.WhenAll(uwp, gdk);

        return new VersionRegistry(registry);
    });

    /*
        - This might be a "micro-optimization".
        - We can avoid using `System.Version` to avoid potential overheads.
    */

    sealed class VersionItemKeyComparer : IComparer<string>
    {
        unsafe readonly struct VersionItemKey
        {
            internal VersionItemKey(string version)
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
            VersionItemKey a = new(x), b = new(y);

            if (a._major != b._major)
                return a._major.CompareTo(b._major);

            if (a._minor != b._minor)
                return a._minor.CompareTo(b._minor);

            return a._build.CompareTo(b._build);
        }
    }

    static readonly VersionItemKeyComparer s_comparer = new();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, VersionItem?>> GetEnumerator() => _registry.OrderByDescending(static _ => _.Key, s_comparer).GetEnumerator();
}