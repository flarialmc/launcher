using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.Win32.Foundation;
using static Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Interface;

sealed class MainWindow : Window
{
    internal MainWindow(ApplicationSettings settings)
    {
        WindowInteropHelper helper = new(this);
        var hwnd = (HWND)helper.EnsureHandle();

        var source = HwndSource.FromHwnd(hwnd);
        source.AddHook(HwndSourceHook);

        unsafe
        {
            BOOL attribute = true;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &attribute, (uint)sizeof(BOOL));
        }

        using (var stream = ApplicationManifest.GetResourceStream("Application.ico"))
        {
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            Icon.Freeze();
        }

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        ResizeMode = ResizeMode.CanMinimize;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Title = "Flarial Launcher";
        Content = new XamlHost(~new MainNavigationView(settings))
        {
            Width = 960,
            Height = 540,
            Focusable = true
        };
    }

    static nint HwndSourceHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (handled || msg != WM_SYSCOMMAND)
            return new();

        switch ((uint)wParam & 0xFFF0)
        {
            case SC_KEYMENU or SC_MOUSEMENU:
                handled = true;
                break;
        }

        return new();
    }
}