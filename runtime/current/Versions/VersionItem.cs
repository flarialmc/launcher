using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Flarial.Launcher.Runtime.Game;
using static Windows.Win32.Foundation.WIN32_ERROR;
using Flarial.Launcher.Runtime.Services;

namespace Flarial.Launcher.Runtime.Versions;

public abstract class VersionItem
{
    readonly string _version;

    internal VersionItem(string version) => _version = version;

    public override string ToString() => _version;

    static readonly string s_path = Path.GetTempPath();

    protected abstract Task<string> GetUrlAsync();

    public virtual async Task InstallAsync(Action<int, bool> action)
    {
        if (!Minecraft.IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!Minecraft.IsPackaged)
            throw new Win32Exception((int)ERROR_UNSIGNED_PACKAGE_INVALID_CONTENT);

        var path = Path.Combine(s_path, Path.GetRandomFileName());
        await HttpService.DownloadAsync(await GetUrlAsync(), path, (_) => action(_, false));
        await Task.Run(() => PackageService.Add(new(path), (_) => action(_, true)));
    }
}