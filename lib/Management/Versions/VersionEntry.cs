using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Core;
using Flarial.Launcher.Services.Networking;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Foundation;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WIN32_ERROR;

namespace Flarial.Launcher.Services.Management.Versions;

public abstract class VersionEntry
{
    internal VersionEntry() { }

    static readonly string s_path = Path.GetTempPath();
    private protected static readonly DataContractJsonSerializerSettings s_settings = new()
    {
        UseSimpleDictionaryFormat = true
    };

    /*
        - Reuse the PackageManager instance from the `Minecraft` class.
    */

    static readonly PackageManager s_manager = Minecraft.s_manager;

    /*
        - This method is present for compatibility reasons.
        - This method is used to resolve the download URI of a package.
    */

    public abstract Task<string> GetAsync();

    public virtual async Task InstallAsync(Action<AppInstallState, int> action) => await Task.Run(async () =>
    {
        if (!Minecraft.IsInstalled)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!Minecraft.IsPackaged)
            throw new Win32Exception((int)ERROR_UNSIGNED_PACKAGE_INVALID_CONTENT);

        var uri = await GetAsync();
        var path = Path.Combine(s_path, Path.GetRandomFileName());

        try
        {
            await HttpService.DownloadAsync(uri, path, (_) => action(AppInstallState.Downloading, _));
            unsafe
            {
                /*
                    - Workaround this issue: https://github.com/microsoft/CsWinRT/issues/1720
                    - We wrap the asynchronous operation as a synchronous operation & proxy it to 'Task.Run()'.
                */

                var @event = CreateEvent(null, true, false, null);
                var item = s_manager.AddPackageAsync(new(path), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

                try
                {
                    item.Progress += (sender, args) => action(AppInstallState.Installing, (int)args.percentage);
                    item.Completed += (_, _) => SetEvent(@event);

                    WaitForSingleObject(@event, INFINITE);
                    if (item.Status is AsyncStatus.Error) throw item.ErrorCode;
                }
                finally { CloseHandle(@event); item.Close(); }
            }
        }
        finally { try { File.Delete(path); } catch { } }
    });
}