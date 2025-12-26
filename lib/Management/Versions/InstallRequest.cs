using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Management.Deployment;
using static Windows.Management.Deployment.DeploymentOptions;
using static System.Threading.Tasks.TaskContinuationOptions;
using static Windows.Foundation.AsyncStatus;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class InstallRequest : IDisposable
{
    static readonly PackageManager s_manager = new();
    static readonly string s_path = Path.GetTempPath();

    readonly Task<bool> _task;
    readonly CancellationTokenSource _source = new();
    readonly string _path = Path.Combine(s_path, Path.GetRandomFileName());

    static async Task<bool> Task(string uri, string path, Action<int> action, CancellationToken token)
    {
        await HttpService.DownloadAsync(uri, path, (_) => action(_ * 90 / 100), token);
        if (token.IsCancellationRequested) return false;

        TaskCompletionSource<bool> source = new();
        var operation = s_manager.AddPackageAsync(new(path), null, ForceApplicationShutdown | ForceUpdateFromAnyVersion);

        operation.Progress += (sender, args) => action(90 + (int)(args.percentage * 10 / 100));

        operation.Completed += (sender, args) =>
        {
            if (sender.Status is Error) source.TrySetException(sender.ErrorCode);
            else source.TrySetResult(true);
        };

        await source.Task; return !token.IsCancellationRequested;
    }

    internal InstallRequest(string uri, Action<int> action)
    {
        _task = Task(uri, _path, action, _source.Token);
        _task.ContinueWith(_ => { try { File.Delete(_path); } catch { } }, ExecuteSynchronously);
    }

    public TaskAwaiter<bool> GetAwaiter() => _task.GetAwaiter();

    public bool Cancel()
    {
        if (_task.IsCompleted) return false;
        _source.Cancel(); return true;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this); _source.Dispose();
        try { File.Delete(_path); } catch { }
    }

    ~InstallRequest() => Dispose();
}