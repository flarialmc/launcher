using Windows.Win32;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32Window
{
    readonly HWND _handle = HWND.Null;

    internal readonly uint ProcessId;

    Win32Window(HWND handle)
    {
        uint processId = 0; GetWindowThreadProcessId(handle, &processId);
        _handle = handle; ProcessId = processId;
    }

    internal void Switch() => PInvoke.SwitchToThisWindow(_handle, true);

    public static implicit operator HWND(Win32Window @this) => @this._handle;

    public static implicit operator Win32Window(HWND @this) => new(@this);
}