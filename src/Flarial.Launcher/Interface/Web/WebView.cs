using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Foundation;
using Windows.Web.UI.Interop;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;

namespace Flarial.Launcher.Interface.Web;

sealed class WebView : HwndHost
{
    static readonly WebViewControlProcess s_process = new();

    internal Task Task { get; set; } = null!;

    internal WebViewControl Current { get; private set; } = null!;

    void OnProcessExit([Optional] object sender, [Optional] EventArgs args)
    {
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        try { Current.Close(); } catch { }
    }

    internal WebView() => AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

    async Task CreateAsync(nint handle)
    {
        Current = await s_process.CreateWebViewControlAsync(handle, new());

        DpiChanged += OnSizeChanged;
        SizeChanged += OnSizeChanged;

        OnSizeChanged();
    }

    protected unsafe override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        fixed (char* @class = "Static")
        {
            var parent = (HWND)hwndParent.Handle;
            var child = CreateWindowEx(0, @class, null, WS_VISIBLE | WS_CHILD, 0, 0, 0, 0, parent);

            Task = CreateAsync(child);
            return new(null, child);
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        OnProcessExit();
        DestroyWindow(new(Handle));
    }

    void OnSizeChanged([Optional] object sender, [Optional] EventArgs args)
    {
        var dpi = VisualTreeHelper.GetDpi(this);

        var width = dpi.DpiScaleX * ActualWidth;
        var height = dpi.DpiScaleY * ActualHeight;

        Current.Bounds = new(0, 0, width, height);
    }
}