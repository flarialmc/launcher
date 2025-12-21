using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.System;

unsafe readonly struct Win32Window
{
    readonly HWND _handle;

    internal uint ProcessId
    {
        get
        {
            uint processId = 0;
            GetWindowThreadProcessId(_handle, &processId);
            return processId;
        }
    }

    Win32Window(HWND handle) => _handle = handle;

    internal void Switch() => SwitchToThisWindow(_handle, true);

    public static implicit operator HWND(Win32Window @this) => @this._handle;

    public static implicit operator Win32Window(HWND @this) => new(@this);
}