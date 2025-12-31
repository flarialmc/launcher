using System.Windows;
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

        Icon = ApplicationManifest.Icon;
        Title = $"Flarial Launcher";
        Width = 960; Height = 540;
        UseLayoutRounding = SnapsToDevicePixels = true;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ResizeMode = ResizeMode.CanMinimize;

        Content = new MainWindowContent(configuration);
    }
}