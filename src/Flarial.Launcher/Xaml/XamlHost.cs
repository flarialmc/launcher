using System.Runtime.InteropServices;
using System.Windows.Interop;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.Win32.Foundation;
using Windows.Win32.System.WinRT.Xaml;
using static Windows.Win32.Graphics.Gdi.GET_STOCK_OBJECT_FLAGS;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.GET_CLASS_LONG_INDEX;
namespace Flarial.Launcher.Xaml;

sealed class XamlHost : HwndHost
{
    readonly DesktopWindowXamlSource _dwxs;
    readonly IDesktopWindowXamlSourceNative _dwxsn;

    internal XamlHost(UIElement element)
    {
        _dwxs = new() { Content = element };
        _dwxsn = (IDesktopWindowXamlSourceNative)_dwxs;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _dwxsn.AttachToWindow((HWND)hwndParent.Handle);
        HandleRef handle = new(this, _dwxsn.WindowHandle);

        var value = GetStockObject(BLACK_BRUSH);
        SetClassLongPtr(_dwxsn.WindowHandle, GCLP_HBRBACKGROUND, value);

        SwitchToThisWindow(_dwxsn.WindowHandle, true);
        return handle;
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => _dwxs.Dispose();
}