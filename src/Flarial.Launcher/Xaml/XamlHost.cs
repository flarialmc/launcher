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
    readonly DesktopWindowXamlSource _host;

    internal XamlHost(UIElement element) => _host = new() { Content = element };

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var host = (IDesktopWindowXamlSourceNative)_host;
        host.AttachToWindow((HWND)hwndParent.Handle);

        var handle = host.WindowHandle;
        SwitchToThisWindow(handle, true);

        var value = GetStockObject(BLACK_BRUSH);
        SetClassLongPtr(handle, GCLP_HBRBACKGROUND, value);

        return new(this, handle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => _host.Dispose();
}
