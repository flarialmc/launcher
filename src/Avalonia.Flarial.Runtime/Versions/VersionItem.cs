using System;
using System.IO;
using System.Threading.Tasks;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public abstract class VersionItem
{
    readonly string _string;

    internal VersionItem(string version) => _string = Stringify(version);

    public override string ToString() => _string;

    static readonly string s_temp = Path.GetTempPath();

    protected abstract Task<string> GetUriAsync();

    internal static string Stringify(string version)
    {
        NumericVersion key = new(version);
        return key.Minor >= 26 ? $"{key.Minor}.{key.Build}" : version;
    }

    public virtual async Task InstallAsync(Action<int, bool> callback)
    {
        if (!Minecraft.IsInstalled)
            throw new MinecraftNotInstalledException();

        if (!Minecraft.IsPackaged)
            throw new MinecraftUnpackagedException();

        var path = Path.Combine(s_temp, Path.GetRandomFileName());
        try
        {
            await HttpService.DownloadAsync(await GetUriAsync(), path, _ => callback(_, false));
            await PackageService.AddAsync(new(path), _ => callback(_, true));
        }
        finally
        {
            try { File.Delete(path); }
            catch { }
        }
    }
}