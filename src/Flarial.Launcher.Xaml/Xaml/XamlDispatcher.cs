using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Flarial.Launcher.Xaml;

static class XamlDispatcher
{
    internal static void Invoke(this CoreDispatcher dispatcher, DispatchedHandler callback, [Optional] CoreDispatcherPriority priority)
    {
        if (dispatcher.HasThreadAccess)
        {
            if (!dispatcher.ShouldYield(priority)) callback();
            else _ = dispatcher.RunAsync(priority, callback);
        }
        else dispatcher.InvokeAsync(callback, priority).GetAwaiter().GetResult();
    }

    internal static async Task InvokeAsync(this CoreDispatcher dispatcher, DispatchedHandler callback, [Optional] CoreDispatcherPriority priority)
    {
        TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = dispatcher.RunAsync(priority, () =>
        {
            try { callback(); tcs.TrySetResult(true); }
            catch (Exception exception) { tcs.TrySetException(exception); }
        });

        await tcs.Task;
    }
}