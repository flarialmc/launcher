using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Services.System;

readonly ref struct Win32Process : IDisposable
{
    readonly HANDLE _handle;

    internal Win32Process(uint processId) => _handle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);

    internal bool IsRunning(uint timeout) => WaitForSingleObject(_handle, timeout ) is WAIT_TIMEOUT;

    public void Dispose() => CloseHandle(_handle);

    public static implicit operator HANDLE(in Win32Process @this) => @this._handle;
}