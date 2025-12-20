using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using Windows.Win32.System.Threading;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using Windows.Win32;

namespace Flarial.Launcher.Services.System;

readonly partial struct Win32Process : IDisposable
{
    readonly HANDLE _handle;
    internal readonly uint Id;

    Win32Process(uint processId, HANDLE handle) => (Id, _handle) = (processId, handle);

    internal static Win32Process? Open(PROCESS_ACCESS_RIGHTS access, uint processId)
    {
        var handle = OpenProcess(access, false, processId);
        return handle != Null ? new(processId, handle) : null;
    }

    internal bool IsRunning => WaitForSingleObject(_handle, 1) is WAIT_TIMEOUT;

    internal void WaitForExit() => WaitForSingleObject(_handle, INFINITE);

    internal void WaitForInputIdle() => PInvoke.WaitForInputIdle(_handle, INFINITE);

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in Win32Process @this) => @this._handle;
}
