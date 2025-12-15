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
    internal readonly uint ProcessId;

    Win32Process(uint processId, HANDLE handle) => (ProcessId, _handle) = (processId, handle);

    internal static Win32Process? Open(PROCESS_ACCESS_RIGHTS access, uint processId)
    {
        var handle = OpenProcess(access, false, processId);
        return handle != Null ? new(processId, handle) : null;
    }

    internal bool Wait(uint timeout) => WaitForSingleObject(_handle, timeout) is WAIT_TIMEOUT;

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in Win32Process @this) => @this._handle;
}

partial struct Win32Process
{
    internal static Win32Process? Open(uint? processId)
    {
        if (processId is not { } @_) return null;
        var handle = OpenProcess(PROCESS_ALL_ACCESS, false, @_);
        return handle != Null ? new(@_, handle) : null;
    }

    internal void WaitForExit() => WaitForSingleObject(_handle, INFINITE);

    internal void WaitForInputIdle() => PInvoke.WaitForInputIdle(_handle, INFINITE);
}