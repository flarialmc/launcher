using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Flarial.Launcher.Services.Networking;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace Flarial.Launcher.Services.Management.Versions;

public sealed class InstallRequest
{
    static readonly PackageManager s_manager = new();
    static readonly string s_path = Path.GetTempPath();
    static readonly ConcurrentDictionary<string, bool> s_paths = [];
    static readonly AddPackageOptions s_options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

    static InstallRequest() => AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    {
        foreach (var path in s_paths)
            try { File.Delete(path.Key); }
            catch { }
    };

    public static async Task InstallAsync(string uri, string path, Action<int> action)
    {
        TaskCompletionSource<bool> source = new();
        await HttpService.DownloadAsync(uri, path, (_) => action(_ * 90 / 100));

        var operation = s_manager.AddPackageByUriAsync(new(path), s_options);
        operation.Progress += (sender, args) => action(90 + ((int)args.percentage * 10 / 100));

        operation.Completed += (sender, args) =>
        {
            if (sender.Status is AsyncStatus.Error) source.TrySetException(sender.ErrorCode);
            else source.TrySetResult(new());
        };

        await source.Task;
    }

    readonly Task _task;
    readonly string _path;

    internal InstallRequest(string uri, Action<int> action)
    {
        _path = Path.Combine(s_path, Path.GetRandomFileName());
        s_paths.TryAdd(_path, new());

        _task = InstallAsync(uri, _path, action);
        _task.ContinueWith(delegate
        {
            try
            {
                File.Delete(_path);
                s_paths.TryRemove(_path, out _);
            }
            catch { }
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    public TaskAwaiter GetAwaiter() => _task.GetAwaiter();

    ~InstallRequest()
    {
        try
        {
            File.Delete(_path);
            s_paths.TryRemove(_path, out _);
        }
        catch { }
    }
}