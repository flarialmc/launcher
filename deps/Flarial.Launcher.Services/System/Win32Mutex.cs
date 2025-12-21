using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.Foundation.HANDLE;
using static Windows.Win32.PInvoke;
using static Flarial.Launcher.Services.System.Win32Process;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.DUPLICATE_HANDLE_OPTIONS;

namespace Flarial.Launcher.Services.System;

readonly unsafe struct Win32Mutex : IDisposable
{
    readonly HANDLE _handle;
    internal readonly bool Exists;

    internal Win32Mutex(string name)
    {
        fixed (char* buffer = name) _handle = CreateMutex(null, false, buffer);
        Exists = _handle != Null && Marshal.GetLastWin32Error() > 0;
    }

    internal bool Duplicate(uint processId)
    {
        if (Open(PROCESS_DUP_HANDLE, processId) is not { } process)
            return false;

        using (process)
        {
            HANDLE target = Null;
            try { return DuplicateHandle(GetCurrentProcess(), _handle, process, &target, 0, false, DUPLICATE_SAME_ACCESS); }
            finally { CloseHandle(target); }
        }
    }

    public void Dispose() => CloseHandle(_handle);
}