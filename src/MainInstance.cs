using System;
using System.Threading;
using System.Windows;

namespace Flarial.Launcher;

static class MainInstance
{
    const string EventName = "72D6883B-C21D-48D9-923A-97D1C32D48C0";

    static readonly EventWaitHandle _handle = new(false, EventResetMode.AutoReset, EventName);

    static MainInstance() => new Thread(Start)
    {
        IsBackground = true,
        Priority = ThreadPriority.Lowest
    }.Start();

    static void Start()
    {
        while (_handle.WaitOne())
            Activated.Invoke();
    }

    internal static event Action Activated;

    internal static void Activate() => _handle.Set();
}