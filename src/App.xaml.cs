using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using Bedrockix.Minecraft;
using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher;

public partial class App : Application
{
    // ↓ Do not modify this initialization code. ↓

    const string Format = @"Looks like the launcher crashed! 
• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}

{3}";

    const string Name = "54874D29-646C-4536-B6D1-8E05053BE00E";

    static readonly Mutex _mutex;

    static void UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var version = $"{Assembly.GetEntryAssembly().GetName().Version}";

        var exception = (Exception)args.ExceptionObject;
        var trace = $"{exception.StackTrace}".Trim();

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var name = exception.GetType().Name;
        var message = exception.Message;

        var text = string.Format(Format, version, name, message, trace);
        MessageBox.Show(text, "Flarial Launcher: Error", MessageBoxButton.OK, MessageBoxImage.Error);

        Environment.Exit(1);
    }

    static App()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        _mutex = new(false, Name, out var value);
        if (!value) using (_mutex) Environment.Exit(0);
    }

    // ↓ Start writing code from here. ↓

    void ApplicationStartup(object sender, StartupEventArgs args)
    {
        var arguments = args.Args;
        var length = arguments.Length;

        var hardwareAcceleration = true;

        for (var index = 0; index < length; index++)
        {
            var argument = arguments[index];
            switch (argument)
            {
                case "--inject":
                    if (!(index + 1 < length))
                        continue;

                    var offset = index + 1;
                    var count = length - offset;

                    ArraySegment<string> segment = new(arguments, offset, count);
                    Loader.Launch(segment);

                    Environment.Exit(0);
                    break;

                case "--no-hardware-acceleration":
                    hardwareAcceleration = false;
                    break;
            }
        }

        Environment.CurrentDirectory = Directory.CreateDirectory(VersionManagement.launcherPath).FullName;
        Directory.CreateDirectory(BackupManager.backupDirectory);
        Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Versions");
        Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Logs");
        string path = @$"{VersionManagement.launcherPath}\cachedToken.txt";

        if (!File.Exists(path))
            File.WriteAllText(path, string.Empty);

        var settings = Settings.Current;
        if (!hardwareAcceleration) settings.HardwareAcceleration = false;
    }
}