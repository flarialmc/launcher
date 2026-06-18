using System;
using System.IO;
using System.Threading.Tasks;
using Flarial.Runtime.Game;
using Flarial.Runtime.Services;

namespace Flarial.Runtime.Versions;

public sealed class InstallRequest
{
    static readonly string s_path = Path.GetTempPath();

    readonly string _downloadUri;
    readonly byte[] _gameLaunchHelper;

    internal InstallRequest(string downloadUri, byte[] gameLaunchHelper)
    {
        _downloadUri = downloadUri;
        _gameLaunchHelper = gameLaunchHelper;
    }

    public async Task InvokeAsync(Action<int, bool> callback)
    {
        var packagePath = Path.Combine(s_path, Path.GetRandomFileName());
     
        try
        {
            await HttpService.DownloadAsync(_downloadUri, packagePath, OnDownload);
            await PackageService.AddAsync(packagePath, OnInstall);

            var installedPath = Minecraft.Package.InstalledPath;
            var gameLaunchHelperPath = Path.Combine(installedPath, "gamelaunchhelper.dll");

            await File.WriteAllBytesAsync(gameLaunchHelperPath, _gameLaunchHelper);
        }
        finally
        {
            try { File.Delete(packagePath); }
            catch { }
        }

        void OnInstall(int value) => callback(value, true);
        void OnDownload(int value) => callback(value, false);
    }
}