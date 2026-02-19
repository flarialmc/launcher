using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Management;
using Flarial.Launcher.Runtime.Game;
using Flarial.Launcher.Runtime.Modding;
using Microsoft.VisualBasic;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher;

static class Program
{
    const string Format = @"Looks like the launcher crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}

{3}";

    static Program()
    {
        NativeMethods.SetErrorMode();
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    static void OnUnhandledException(Exception exception)
    {
        var trace = exception.StackTrace.Trim();

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var name = exception.GetType().Name;
        var message = exception.Message;

        var text = string.Format(Format, ApplicationManifest.s_version, name, message, trace);
        MessageBox.Show(text, "Flarial Launcher: Error", MessageBoxButton.OK, MessageBoxImage.Error);

        Exit(1);
    }

    static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs args)
    {
        OnUnhandledException((Exception)args.ExceptionObject);
    }

    static void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        args.Handled = true;
        OnUnhandledException(args.Exception);
    }

    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(false, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created) return;

        var path = Path.Combine(GetFolderPath(LocalApplicationData), @"Flarial\Launcher");
        CurrentDirectory = Directory.CreateDirectory(path).FullName;

        var settings = ApplicationSettings.ReadSettings();

        for (var index = 0; index < args.Length; index++)
            switch (args[index])
            {
                case "--inject":
                    if (!(index + 1 < args.Length))
                        continue;

                    Injector.Launch(true, new(args[index + 1]));
                    return;

                case "--allow-unsigned-installs":
                    Minecraft.AllowUnsignedInstalls = true;
                    break;
            }

        using (WindowsXamlManager.InitializeForCurrentThread())
        {
            var application = Windows.UI.Xaml.Application.Current;
            ColorPaletteResources resources = new() { Accent = Colors.IndianRed };

            application.RequestedTheme = ApplicationTheme.Dark;
            application.UnhandledException += OnUnhandledException;
            application.Resources.MergedDictionaries.Add(resources);

            new MainApplication(settings).Run(new MainWindow(settings));
        }
    }
}