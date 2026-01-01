using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static System.Threading.Tasks.TaskContinuationOptions;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class InstallRequest
{
    static readonly PackageManager s_manager = new();
    static readonly string s_path = Path.GetTempPath();

    readonly Task _task;

    internal InstallRequest(string uri, Action<AppInstallState, int> action)
    {
        var path = Path.Combine(s_path, Path.GetRandomFileName());

        _task = Task.Run(async () =>
        {
            await HttpService.DownloadAsync(uri, path, (_) => action(AppInstallState.Downloading, _));
            var item = s_manager.AddPackageAsync(new(path), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

            unsafe
            {
                /*
                    - Workaround this issue: https://github.com/microsoft/CsWinRT/issues/1720
                    - We wrap the asynchronous operation as a synchronous operation & proxy it to 'Task.Run()'.
                */

                var @event = CreateEvent(null, true, false, null); try
                {
                    item.Progress += (sender, args) => action(AppInstallState.Installing, (int)args.percentage);
                    item.Completed += (_, _) => SetEvent(@event);

                    WaitForSingleObject(@event, INFINITE);
                    if (item.Status is AsyncStatus.Error) throw item.ErrorCode;
                }
                finally { CloseHandle(@event); item.Close(); }
            }
        });

        _task.ContinueWith(_ =>
        {
            try { File.Delete(path); }
            catch { }
        }, ExecuteSynchronously);
    }

    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();
}