using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Runtime.System;

unsafe readonly struct NativeWindow
{
    readonly HWND _handle;

    NativeWindow(HWND handle) => _handle = handle;

    internal void Switch() => SwitchToThisWindow(_handle, true);

    internal uint ProcessId { get { uint _; GetWindowThreadProcessId(_handle, &_); return _; } }

    public static implicit operator NativeWindow(in HWND @this) => new(@this);

    public static implicit operator HWND(in NativeWindow @this) => @this._handle;
}