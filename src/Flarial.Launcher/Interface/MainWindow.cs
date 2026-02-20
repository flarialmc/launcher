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
    static nint HwndSourceHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WM_SYSCOMMAND)
        {
            var command = (uint)wParam & 0xFFF0;
            handled = command is SC_KEYMENU or SC_MOUSEMENU;
        }
        return new();
    }

    internal MainWindow(ApplicationSettings settings)
    {
        WindowInteropHelper helper = new(this);
        var handle = (HWND)helper.EnsureHandle();

        var source = HwndSource.FromHwnd(handle);
        source.AddHook(HwndSourceHook);

        unsafe
        {
            BOOL attribute = true;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, &attribute, (uint)sizeof(BOOL));
        }

        using (var stream = ApplicationManifest.GetResourceStream("Application.ico"))
        {
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            Icon.Freeze();
        }

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;

        Title = "Flarial Launcher";
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = new XamlHost(new MainNavigationView(settings)._this) { Width = 960, Height = 540 };
    }
}