using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.System;

unsafe readonly struct NativeWindow
{
    readonly HWND _handle;

    NativeWindow(HWND handle) => _handle = handle;

    internal bool IsVisible => IsWindowVisible(_handle);

    internal void Switch() => SwitchToThisWindow(_handle, true);

    internal uint ProcessId
    {
        get
        {
            uint processId;
            GetWindowThreadProcessId(_handle, &processId);
            return processId;
        }
    }

    public static implicit operator NativeWindow(in HWND hwnd) => new(hwnd);

    public static implicit operator HWND(in NativeWindow window) => window._handle;
}