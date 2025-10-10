using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Flarial.Launcher;

readonly struct NotifyCompletion : INotifyCompletion
{
    public void OnCompleted(Action continuation)
    {
        var context = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            continuation();
        }
        finally { SynchronizationContext.SetSynchronizationContext(context); }
    }

    public void GetResult() { }

    internal NotifyCompletion GetAwaiter() => this;

    internal bool IsCompleted => SynchronizationContext.Current is null;
}