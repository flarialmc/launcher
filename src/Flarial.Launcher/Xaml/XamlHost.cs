using System.Runtime.InteropServices;
using System.Windows.Interop;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.Win32.Foundation;
using Windows.Win32.System.WinRT.Xaml;
using static Windows.Win32.Graphics.Gdi.GET_STOCK_OBJECT_FLAGS;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.GET_ANCESTOR_FLAGS;
using static Windows.Win32.UI.WindowsAndMessaging.GET_CLASS_LONG_INDEX;

namespace Flarial.Launcher.Xaml;

sealed class XamlHost(UIElement content) : HwndHost
{
    readonly DesktopWindowXamlSource _xaml = new() { Content = content };

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var native = (IDesktopWindowXamlSourceNative)_xaml;

        native.AttachToWindow(new(hwndParent.Handle));
        SetClassLongPtr(native.WindowHandle, GCLP_HBRBACKGROUND, GetStockObject(BLACK_BRUSH));

        return new(this, native.WindowHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd) => _xaml.Dispose();
}