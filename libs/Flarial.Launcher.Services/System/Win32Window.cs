using System.Deployment.Internal;
using Windows.Win32.Foundation;
using Windows.Win32;
using static Windows.Win32.PInvoke;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.UnmanagedType;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32Window
{
    readonly HWND _handle = HWND.Null;

    readonly uint _processId = new();

    [DllImport("User32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(Bool)]
    static extern bool EndTask(nint hWnd, [MarshalAs(Bool)] bool fShutDown, [MarshalAs(Bool)] bool fForce);

    Win32Window(HWND handle)
    {
        uint processId = 0;
        GetWindowThreadProcessId(handle, &processId);

        _handle = handle;
        _processId = processId;
    }

    internal Win32Process Process => new(_processId);

    internal void SetForeground() => SetForegroundWindow(_handle);

    internal void EndTask() => EndTask(_handle, false, true);

    public static implicit operator HWND(Win32Window @this) => @this._handle;

    public static implicit operator Win32Window(HWND @this) => new(@this);
}