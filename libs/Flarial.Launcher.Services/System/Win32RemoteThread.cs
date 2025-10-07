using System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32RemoteThread : IDisposable
{
    readonly void* _parameter;

    readonly HANDLE _thread, _process;

    internal Win32RemoteThread(in Win32Process process, LPTHREAD_START_ROUTINE routine, string parameter)
    {
        fixed (char* buffer = parameter)
        {
            const PAGE_PROTECTION_FLAGS flags = PAGE_PROTECTION_FLAGS.PAGE_READWRITE;
            const VIRTUAL_ALLOCATION_TYPE type = VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE;


            _process = process;
            var size = (nuint)(parameter.Length + 1) * sizeof(char);

            _parameter = VirtualAllocEx(process, null, size, type, flags);
            WriteProcessMemory(process, _parameter, buffer, size, null);

            _thread = CreateRemoteThread(_process, null, 0, routine, _parameter, 0, null);
            WaitForSingleObject(_thread, INFINITE);
        }
    }

    public void Dispose()
    {
        const VIRTUAL_FREE_TYPE type = VIRTUAL_FREE_TYPE.MEM_RELEASE;
        CloseHandle(_thread); VirtualFreeEx(_process, _parameter, 0, type);
    }
}