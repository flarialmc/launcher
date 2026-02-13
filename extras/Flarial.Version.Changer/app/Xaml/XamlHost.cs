using System.Runtime.InteropServices;
using System.Windows.Interop;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.Win32.Foundation;
using Windows.Win32.System.WinRT.Xaml;

sealed class XamlHost : HwndHost
{
    readonly DesktopWindowXamlSource _dwxs = new();

    internal XamlHost(XamlElement element) => _dwxs.Content = (UIElement)element;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        var dwxsn = (IDesktopWindowXamlSourceNative)_dwxs;
        dwxsn.AttachToWindow((HWND)hwndParent.Handle);
        return new(this, dwxsn.WindowHandle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        _dwxs.Dispose();
    }
}