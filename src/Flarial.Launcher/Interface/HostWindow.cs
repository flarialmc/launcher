using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Flarial.Launcher.Controls;
using Flarial.Launcher.Management;
using Flarial.Launcher.Xaml;
using Windows.Win32.Foundation;
using static Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE;
using static Windows.Win32.PInvoke;

namespace Flarial.Launcher.Interface;

sealed class HostWindow : Window
{
    internal HostWindow(AppSettings settings)
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

        using (var stream = AppManifest.GetStream("Application.ico"))
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

        const double sideWidth = 200;
        const double centerWidth = 960;
        const double height = 540;

        Grid grid = new() { Background = Brushes.Black };
        grid.ColumnDefinitions.Add(new() { Width = new(sideWidth) });
        grid.ColumnDefinitions.Add(new() { Width = new(centerWidth) });
        grid.ColumnDefinitions.Add(new() { Width = new(sideWidth) });

        var leftAd = new BannerAdsView(0) { Width = sideWidth, Height = height };
        Grid.SetColumn(leftAd, 0);
        grid.Children.Add(leftAd);

        var host = new XamlHost(~new XamlContent(settings))
        {
            Width = centerWidth,
            Height = height,
            Focusable = true
        };
        Grid.SetColumn(host, 1);
        grid.Children.Add(host);

        var rightAd = new BannerAdsView(1) { Width = sideWidth, Height = height };
        Grid.SetColumn(rightAd, 2);
        grid.Children.Add(rightAd);

        Content = grid;
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