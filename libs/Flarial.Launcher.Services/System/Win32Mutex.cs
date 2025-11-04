using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static System.Runtime.InteropServices.Marshal;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.Foundation.DUPLICATE_HANDLE_OPTIONS;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Launcher.Services.System;

unsafe readonly ref struct Win32Mutex : IDisposable
{
    static readonly HANDLE _process = GetCurrentProcess();

    readonly HANDLE _handle = Null;

    internal Win32Mutex(string name)
    {
        fixed (char* buffer = name) _handle = CreateMutex(null, false, buffer);
        Exists = _handle != INVALID_HANDLE_VALUE && GetLastWin32Error() > 0;
    }

    internal readonly bool Exists;

    internal bool Duplicate(uint processId)
    {
        if (Win32Process.Open(PROCESS_DUP_HANDLE, processId) is not { } process)
            return false;

        using (process)
        {
            var handle = Null;
            try { return DuplicateHandle(_process, _handle, process, &handle, 0, false, DUPLICATE_SAME_ACCESS); }
            finally { CloseHandle(handle); }
        }
    }

    public void Dispose() => CloseHandle(_handle);
}
