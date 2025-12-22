using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Services.Native;

unsafe readonly struct NativeWindow
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

    NativeWindow(HWND handle) => _handle = handle;

    internal void Switch() => SwitchToThisWindow(_handle, true);

    public static implicit operator HWND(in NativeWindow @this) => @this._handle;

    public static implicit operator NativeWindow(in HWND @this) => new(@this);
}