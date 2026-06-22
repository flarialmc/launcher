using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public sealed class VersionItem
{
    static readonly string s_path = Path.GetTempPath();

    public override string ToString() => _string;

    internal VersionItem(string version, string[] downloadUris, byte[] gameLaunchHelper)
    {
        _downloadUris = downloadUris;
        _gameLaunchHelper = gameLaunchHelper;
        _string = new GameVersion(version).ToString();
    }

    readonly string _string;
    readonly string[] _downloadUris;
    readonly byte[] _gameLaunchHelper;

    async Task InstallAsync(HttpResponseMessage response, Action<int, bool> callback)
    {
        using (response)
        {
            var packagePath = Path.Combine(s_path, Path.GetRandomFileName());
            try
            {
                await HttpService.DownloadAsync(response, packagePath, OnDownload);
                await Task.Run(() => PackageService.Add(packagePath, OnInstall));

                var installedPath = Minecraft.Package.InstalledPath;
                var gameLaunchHelperPath = Path.Combine(installedPath, "gamelaunchhelper.dll");

                await File.WriteAllBytesAsync(gameLaunchHelperPath, _gameLaunchHelper);
            }
            finally
            {
                try { File.Delete(packagePath); }
                catch { }
            }
        }

        void OnInstall(int value) => callback(value, true);
        void OnDownload(int value) => callback(value, false);
    }

    public async Task<Task?> InstallAsync(Action<int, bool> callback)
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (!Minecraft.IsInstalled)
            throw new MinecraftNotInstalledException();

        if (Minecraft.IsSideloaded)
            throw new MinecraftSideloadedException();

        var response = await HttpService.ProbeAsync(_downloadUris);
        if (response is null) return null;

        return InstallAsync(response, callback);
    }
}