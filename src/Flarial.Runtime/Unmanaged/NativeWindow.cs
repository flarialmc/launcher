using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Runtime.Unmanaged;

unsafe readonly struct NativeWindow
{
    readonly HWND _handle;
    internal readonly uint _processId;

    internal NativeWindow(HWND handle)
    {
        uint processId = 0;
        GetWindowThreadProcessId(handle, &processId);

        _handle = handle;
        _processId = processId;
    }

    internal bool IsVisible => IsWindowVisible(_handle);
    internal void Switch() => SwitchToThisWindow(_handle, true);

    public static implicit operator NativeWindow(in HWND hwnd) => new(hwnd);
    public static implicit operator HWND(in NativeWindow window) => window._handle;
}