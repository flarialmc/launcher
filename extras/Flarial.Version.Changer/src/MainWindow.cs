using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ModernWpf;
using ModernWpf.Controls.Primitives;

sealed class MainWindow : Window
{
    internal MainWindow()
    {
        WindowHelper.SetUseModernWindowStyle(this, true);
        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Width = 960;
        Height = 540;
        Title = "Flarial Version Changer";

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Application.ico"))
        {
            Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            Icon.Freeze();
        }

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.Manual;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;

        Content = new MainFrame();
    }
}