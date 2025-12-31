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
using System.Threading;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class InstallRequest
{
    static readonly PackageManager s_manager = new();
    static readonly string s_path = Path.GetTempPath();

    readonly Task _task;
    readonly string _uri;
    readonly Action<AppInstallState, int> _action;
    readonly string _path = Path.Combine(s_path, Path.GetRandomFileName());

    async Task CreateRequestAsync()
    {
        await HttpService.DownloadAsync(_uri, _path, OnDownloadProgress);

        State = AppInstallState.Installing;
        var item = s_manager.AddPackageAsync(new(_path), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        unsafe
        {
            /*
                - Workaround this issue: https://github.com/microsoft/CsWinRT/issues/1720
                - We wrap the asynchronous operation as a synchronous operation & proxy it to 'Task.Run()'.
            */

            var @event = CreateEvent(null, true, false, null);

            try
            {
                item.Progress += OnInstallProgress;
                item.Completed += (_, _) => SetEvent(@event);

                WaitForSingleObject(@event, INFINITE);
                if (item.Status is AsyncStatus.Error)
                    throw item.ErrorCode;
            }
            finally
            {
                CloseHandle(@event);
                item.Close();
            }
        }
    }

    void OnDownloadProgress(int value)
    {
        _action(AppInstallState.Downloading, value);
    }

    void OnInstallProgress(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> sender, DeploymentProgress args)
    {
        _action(AppInstallState.Installing, (int)args.percentage);
    }

    void CleanupRequest(Task task)
    {
        try { File.Delete(_path); } finally { }
    }

    internal InstallRequest(string uri, Action<AppInstallState, int> action)
    {
        _uri = uri;
        _action = action;

        _task = Task.Run(CreateRequestAsync);
        _task.ContinueWith(CleanupRequest, ExecuteSynchronously);
    }

    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

    public unsafe AppInstallState State
    {
        get
        {
            fixed (AppInstallState* _ = &field)
            {
                var value = Volatile.Read(ref *(int*)_);
                return *(AppInstallState*)&value;
            }
        }

        private set
        {
            fixed (AppInstallState* _ = &field)
                Interlocked.Exchange(ref *(int*)_, *(int*)&value);
        }
    } = AppInstallState.Downloading;

    ~InstallRequest() { try { File.Delete(_path); } catch { } }
}