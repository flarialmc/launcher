using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Web.UI.Interop;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;

namespace Flarial.Launcher.Xaml;

sealed class WebViewHost : HwndHost
{
    static readonly WebViewControlProcess s_process = new();

    static WebViewHost() => AppDomain.CurrentDomain.ProcessExit += static (_, _) =>
    {
        try { s_process.Terminate(); }
        catch { }
    };

    internal WebViewControl WebView { get; private set; } = null!;

    internal Task Task { get; set { field ??= value; } } = null!;

    async Task CreateAsync(nint handle)
    {
        WebView = await s_process.CreateWebViewControlAsync(handle, new());
        SizeChanged += OnSizeChanged; OnSizeChanged();
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

    protected override void DestroyWindowCore(HandleRef hwnd) => DestroyWindow(new(Handle));

    void OnSizeChanged([Optional] object sender, [Optional] RoutedEventArgs args) => WebView.Bounds = new(0, 0, ActualWidth, ActualHeight);
}