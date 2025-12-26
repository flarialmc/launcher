using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Flarial.Launcher.App;
using ModernWpf;
using ModernWpf.Controls.Primitives;

namespace Flarial.Launcher.UI;

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
        ResizeMode = ResizeMode.NoResize;

        Content = new MainWindowContent(configuration) { IsEnabled = false };
    }
}