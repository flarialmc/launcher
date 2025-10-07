using System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

readonly struct Win32Process : IDisposable
{
    readonly HANDLE _handle;

    internal readonly uint Id;

    internal Win32Process(uint processId)
    {
        const PROCESS_ACCESS_RIGHTS rights = PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS;
        Id = processId; _handle = OpenProcess(rights, false, processId);
    }

    internal bool Running(uint timeout)
    {
        const WAIT_EVENT @event = WAIT_EVENT.WAIT_TIMEOUT;
        return WaitForSingleObject(_handle, timeout) is @event;
    }

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in Win32Process @this) => @this._handle;
}