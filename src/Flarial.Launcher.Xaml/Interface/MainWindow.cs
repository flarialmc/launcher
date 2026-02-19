using System.Drawing;
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
        Title = "Flarial Launcher";

        WindowInteropHelper helper = new(this);
        var handle = (HWND)helper.EnsureHandle();

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

        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = new XamlHost(new MainNavigationView(settings)._this)
        {
            Focusable = true,
            Width = 960,
            Height = 540
        };
    }
}