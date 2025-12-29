using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static System.Threading.Tasks.TaskContinuationOptions;
using static Windows.Foundation.AsyncStatus;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class InstallRequest
{
    const DeploymentOptions Options = ForceApplicationShutdown | ForceUpdateFromAnyVersion;

    static readonly PackageManager s_manager = new();
    static readonly string s_path = Path.GetTempPath();

    readonly Task _task;
    readonly string _path = Path.Combine(s_path, Path.GetRandomFileName());

    static async Task InstallAsync(InstallRequest request, string uri, string path, Action<AppInstallState, int> action)
    {
        request.State = AppInstallState.Downloading;
        await HttpService.DownloadAsync(uri, path, (_) => action(request.State, _));

        TaskCompletionSource<bool> source = new();
        var operation = s_manager.AddPackageAsync(new(path), null, Options);

        request.State = AppInstallState.Installing;
        operation.Progress += (sender, args) => action(request.State, (int)args.percentage);

        operation.Completed += (sender, args) =>
        {
            if (sender.Status != Error) source.TrySetResult(true);
            else source.TrySetException(sender.ErrorCode);
        };

        await source.Task;
        request.State = AppInstallState.Completed;
    }

    internal InstallRequest(string uri, Action<AppInstallState, int> action)
    {
        _task = InstallAsync(this, uri, _path, action);
        _task.ContinueWith(_ => { try { File.Delete(_path); } catch { } }, ExecuteSynchronously);
    }

    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

    public AppInstallState State { get; private set; } = AppInstallState.Pending;

    ~InstallRequest() { try { File.Delete(_path); } catch { } }
}