using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.Interface.Pages;
using Flarial.Launcher.Management;
using ModernWpf;
using ModernWpf.Controls.Primitives;

namespace Flarial.Launcher.Interface;

sealed class MainWindow : Window
{
    internal MainWindow(Configuration configuration)
    {
        WindowHelper.SetUseModernWindowStyle(this, true);
        ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

        Width = 960;
        Height = 540;
        Title = $"Flarial Launcher";
        Icon = ApplicationManifest.Icon;

        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

        Content = new RootPage(configuration, new(this));
    }
}