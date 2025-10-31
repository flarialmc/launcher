using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.UnmanagedType;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32Window
{
    readonly HWND _handle = HWND.Null;

    internal readonly uint ProcessId = new();

    [return: MarshalAs(Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("User32", ExactSpelling = true, SetLastError = true)]
    static extern bool EndTask(nint hWnd, [MarshalAs(Bool)] bool fShutDown, [MarshalAs(Bool)] bool fForce);

    Win32Window(HWND handle)
    {
        uint processId = 0;
        GetWindowThreadProcessId(handle, &processId);
        _handle = handle; ProcessId = processId;
    }

    internal void Switch() => SwitchToThisWindow(_handle, true);

    internal void Close() => EndTask(_handle, false, true);

    public static implicit operator HWND(Win32Window @this) => @this._handle;

    public static implicit operator Win32Window(HWND @this) => new(@this);
}