using System;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;
using static Windows.Win32.Foundation.WAIT_EVENT;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct ProcessHandle : IDisposable
{
    readonly HANDLE _processHandle;

    internal readonly uint ProcessId;

    internal static ProcessHandle? Open(uint processId)
    {
        var processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        return processHandle != HANDLE.INVALID_HANDLE_VALUE ? new(processId, processHandle) : null;
    }

    internal static ProcessHandle? Open(HWND windowHandle)
    {
        uint processId = 0;
        GetWindowThreadProcessId(windowHandle, &processId);
        return Open(processId);
    }

    ProcessHandle(uint processId, HANDLE processHandle)
    {
        ProcessId = processId;
        _processHandle = processHandle;
    }

    internal void Terminate()
    {
        TerminateProcess(_processHandle, 0);
        WaitForSingleObject(_processHandle, INFINITE);
    }

    internal bool WaitForExit() => WaitForSingleObject(_processHandle, INFINITE) is WAIT_EVENT.WAIT_OBJECT_0;

    internal bool IsRunning(uint millseconds) => WaitForSingleObject(_processHandle, millseconds) is WAIT_EVENT.WAIT_TIMEOUT;

    public void Dispose() => CloseHandle(_processHandle);

    public static implicit operator HANDLE(ProcessHandle processHandle) => processHandle._processHandle;
}