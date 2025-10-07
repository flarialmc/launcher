using System;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Flarial.Launcher.SDK;

public sealed partial class Request : IDisposable
{
    readonly WaitHandle Handle;

    readonly TaskCompletionSource<object> Source;

    readonly IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> Operation;

    internal Request(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> operation, Action<int> action = default)
    {
        Handle = ((IAsyncResult)(Source = new()).Task).AsyncWaitHandle;

        (Operation = operation).Completed += (@this, _) =>
        {
            if (@this.Status is AsyncStatus.Error) Source.TrySetException(@this.ErrorCode);
            else Source.TrySetResult(default);
        };

        if (action != default)
            Operation.Progress += (_, value) =>
            {
                if (value.state is DeploymentProgressState.Processing)
                    action((int)value.percentage);
            };
    }

    public partial TaskAwaiter<object> GetAwaiter() => Source.Task.GetAwaiter();

    public partial void Cancel()
    {
        if (Source.Task.IsCompleted) return;
        Operation.Cancel(); Handle.WaitOne();
    }

    public partial void Dispose() { Handle.Dispose(); GC.SuppressFinalize(this); }

    /// <summary>
    /// Releases resources held by the installation request.
    /// </summary>

    ~Request() => Dispose();
}