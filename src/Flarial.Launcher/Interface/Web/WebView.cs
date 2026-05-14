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

    static WebView() => AppDomain.CurrentDomain.ProcessExit += static (_, _) =>
    {
        foreach (var control in s_process.GetWebViewControls()) control.Close();
        try { s_process.Terminate(); } catch { }
    };

    internal Task Task { get; set { field ??= value; } } = null!;

    internal WebViewControl Control { get; private set { field ??= value; } } = null!;

    async Task CreateAsync(nint handle)
    {
        Control = await s_process.CreateWebViewControlAsync(handle, new());

        DpiChanged += OnSizeChanged;
        SizeChanged += OnSizeChanged;

        OnSizeChanged(null!, null!);
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
        Control.Close();
        DestroyWindow(new(Handle));
    }

    void OnSizeChanged(object sender, EventArgs args)
    {
        var dpi = VisualTreeHelper.GetDpi(this);

        var width = dpi.DpiScaleX * ActualWidth;
        var height = dpi.DpiScaleY * ActualHeight;

        Control.Bounds = new(0, 0, width, height);
    }
}