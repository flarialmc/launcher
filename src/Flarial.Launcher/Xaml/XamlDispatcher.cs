using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Flarial.Launcher.Xaml;

static class XamlDispatcher
{
    internal static void Invoke(this CoreDispatcher dispatcher, Action callback, [Optional] CoreDispatcherPriority priority)
    {
        if (dispatcher.HasThreadAccess) callback();
        else dispatcher.InvokeAsync(callback, priority).GetAwaiter().GetResult();
    }

    internal static Task InvokeAsync(this CoreDispatcher dispatcher, Action callback, [Optional] CoreDispatcherPriority priority)
    {
        TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = dispatcher.RunAsync(priority, () =>
        {
            try { callback(); tcs.TrySetResult(true); }
            catch (Exception exception) { tcs.TrySetException(exception); }
        });

        return tcs.Task;
    }
}