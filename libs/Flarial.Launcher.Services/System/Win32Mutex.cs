using System;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static System.Runtime.InteropServices.Marshal;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.Foundation.DUPLICATE_HANDLE_OPTIONS;

namespace Flarial.Launcher.Services.System;

unsafe readonly ref struct Win32Mutex : IDisposable
{
    static readonly HANDLE _process = GetCurrentProcess();

    readonly HANDLE _handle;

    internal Win32Mutex(string name)
    {
        _handle = CreateMutex(null, false, name);
        Exists = _handle != INVALID_HANDLE_VALUE;
        Exists = Exists && GetLastWin32Error() > 0;
    }

    internal readonly bool Exists;

    internal void Duplicate(in Win32Process process)
    {
        HANDLE handle = INVALID_HANDLE_VALUE;
        try { DuplicateHandle(_process, _handle, process, &handle, 0, false, DUPLICATE_SAME_ACCESS); }
        finally { CloseHandle(handle); }
    }

    public void Dispose() => CloseHandle(_handle);
}
