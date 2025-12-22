using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using Windows.Win32.System.Threading;
using static Windows.Win32.Foundation.HANDLE;

namespace Flarial.Launcher.Services.Native;

readonly struct NativeProcess : IDisposable
{
    readonly HANDLE _handle;

    NativeProcess(HANDLE handle) => _handle = handle;

    internal static NativeProcess? Open(PROCESS_ACCESS_RIGHTS access, uint processId)
    {
        var handle = OpenProcess(access, false, processId);
        return handle != Null ? new(handle) : null;
    }

    internal bool Wait(uint timeout) => WaitForSingleObject(_handle, timeout) is WAIT_TIMEOUT;

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in NativeProcess @this) => @this._handle;
}
