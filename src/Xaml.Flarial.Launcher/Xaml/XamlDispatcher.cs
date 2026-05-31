using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Flarial.Launcher.Xaml;

static class XamlDispatcher
{
    extension(CoreDispatcher dispatcher)
    {
        internal void Invoke(Action callback, [Optional] CoreDispatcherPriority priority)
        {
            if (dispatcher.HasThreadAccess) callback();
            else dispatcher.InvokeAsync(callback, priority).GetAwaiter().GetResult();
        }

        internal Task InvokeAsync(Action callback, [Optional] CoreDispatcherPriority priority)
        {
            TaskCompletionSource<bool> source = new(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = dispatcher.RunAsync(priority, () =>
            {
                try
                {
                    callback();
                    source.TrySetResult(true);
                }
                catch (Exception exception)
                {
                    source.TrySetException(exception);
                }
            });

            return source.Task;
        }
    }
}