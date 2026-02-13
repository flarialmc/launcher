using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE;

sealed class MainWindow : Window
{
    internal MainWindow()
    {
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Application.ico"))
        {
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            Icon.Freeze();
        }

        Title = "Flarial Version Changer";
        SizeToContent = SizeToContent.WidthAndHeight;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        unsafe
        {
            WindowInteropHelper helper = new(this);
            var hwnd = (HWND)helper.EnsureHandle();

            BOOL attribute = true;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &attribute, (uint)sizeof(BOOL));
        }

        Content = new XamlHost(new MainFrame()) { Width = 960, Height = 540 };
    }
}