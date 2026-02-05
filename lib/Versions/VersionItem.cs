using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Game;
using Flarial.Launcher.Services.Networking;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Foundation;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WIN32_ERROR;
using System.Linq;

namespace Flarial.Launcher.Services.Versions;

public abstract class VersionItem
{
    internal VersionItem() { }

    static readonly string s_path = Path.GetTempPath();
    protected static readonly PackageManager s_manager = Minecraft.s_manager;
    private protected static readonly DataContractJsonSerializerSettings s_settings = new() { UseSimpleDictionaryFormat = true };

    public abstract Task<string> GetAsync();

    public virtual async Task InstallAsync(Action<int, bool> action) => await Task.Run(async () =>
    {
        if (!Minecraft.Installed)
            throw new Win32Exception((int)ERROR_INSTALL_PACKAGE_NOT_FOUND);

        if (!Minecraft.Packaged)
            throw new Win32Exception((int)ERROR_UNSIGNED_PACKAGE_INVALID_CONTENT);

        var url = await GetAsync();
        var path = Path.Combine(s_path, Path.GetRandomFileName());

        try
        {
            await HttpService.DownloadAsync(url, path, (_) => action(_, false));
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
                    item.Progress += (sender, args) => action((int)args.percentage, true);
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