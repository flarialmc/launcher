using System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.PInvoke;

namespace Flarial.Runtime.Unmanaged;

readonly struct NativeProcess : IDisposable
{
    readonly HANDLE _handle;

    NativeProcess(HANDLE handle) => _handle = handle;

    internal static NativeProcess? Open(PROCESS_ACCESS_RIGHTS access, uint processId)
    {
        var handle = OpenProcess(access, false, processId);
        return !handle.IsNull ? new(handle) : null;
    }

    internal bool Wait(uint timeout) => WaitForSingleObject(_handle, timeout) is WAIT_TIMEOUT;

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in NativeProcess process) => process._handle;
}
