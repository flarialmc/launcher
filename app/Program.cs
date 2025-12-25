using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.App;
using Flarial.Launcher.UI;
using ModernWpf;
using ModernWpf.Controls;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher;

static class Program
{
    static Program() => AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    };

    [STAThread]
    static void Main()
    {
        using (new Mutex(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var value))
        {
            var path = Path.Combine(Environment.GetFolderPath(LocalApplicationData), @"Flarial\Launcher");
            Environment.CurrentDirectory = Directory.CreateDirectory(path).FullName;

            Application application = new();
            var configuration = Configuration.Get();

            application.Resources.MergedDictionaries.Add(new ThemeResources());
            application.Resources.MergedDictionaries.Add(new XamlControlsResources());
            application.Resources.MergedDictionaries.Add(new ColorPaletteResources
            {
                TargetTheme = ApplicationTheme.Dark,
                Accent = Colors.IndianRed,
                AltHigh = Colors.Red
            });

            application.Exit += (_, _) => configuration.Save();
            application.Run(new MainWindow(configuration));
        }
    }
}