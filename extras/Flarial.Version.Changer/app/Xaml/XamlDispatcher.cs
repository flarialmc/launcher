using System;
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.System;

sealed class XamlDispatcher
{
    readonly DispatcherQueue _dispatcher;

    internal XamlDispatcher()
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();
    }

    internal async Task InvokeAsync(Action callback, [Optional] DispatcherQueuePriority priority)
    {
        TaskCompletionSource<bool> tcs = new();

        _dispatcher.TryEnqueue(priority, () =>
        {
            try
            {
                callback();
                tcs.TrySetResult(true);
            }
            catch (Exception exception)
            {
                tcs.TrySetException(exception);
            }
        });

        await tcs.Task;
    }
}