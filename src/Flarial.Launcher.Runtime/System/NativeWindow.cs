using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.System;

unsafe readonly struct NativeWindow(HWND handle)
{
    readonly HWND _handle = handle;

    internal void Switch() => SwitchToThisWindow(_handle, true);

    internal bool IsVisible => IsWindowVisible(_handle);
    internal uint ProcessId { get { uint _; GetWindowThreadProcessId(_handle, &_); return _; } }

    public static implicit operator NativeWindow(in HWND hwnd) => new(hwnd);
    public static implicit operator HWND(in NativeWindow window) => window._handle;
}