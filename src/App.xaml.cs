using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bedrockix.Minecraft;
using Flarial.Launcher.Functions;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher;

public partial class App : Application
{
    // ↓ Do not modify this initialization code. ↓

    static readonly Mutex _mutex;

    [SecurityCritical, HandleProcessCorruptedStateExceptions]
    static void UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        const string format = "Version: {0}\nException: {1}\n\n{2}\n\n{3}";
        var version = $"{Assembly.GetEntryAssembly().GetName().Version}";

        var exception = (Exception)args.ExceptionObject;
        var trace = $"{exception.StackTrace}".Trim();

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var name = exception.GetType().Name;
        var message = exception.Message;

        var caption = Current?.MainWindow is null ? "Flarial Launcher" : "Error";
        var text = string.Format(format, version, name, message, trace);

        MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(0);
    }

    // ↓ Start writing code from here. ↓

    static App()
    {
        // ↓ Do not modify this initialization code. ↓

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        _mutex = new(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var value);
        if (!value) Environment.Exit(0);

        // ↓ Start writing code from here. ↓

        var args = Environment.GetCommandLineArgs();
        for (var index = 0; index + 1 < args.Length; index++)
            switch (args[index])
            {
                case "inject":
                    Loader.Launch(new ArraySegment<string>(args, ++index, args.Length - index));
                    Environment.Exit(default);
                    break;
            }

        Environment.CurrentDirectory = Directory.CreateDirectory(VersionManagement.launcherPath).FullName;
        Directory.CreateDirectory(BackupManager.backupDirectory);
        Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Versions");

        var info = Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Logs");
        string path = @$"{VersionManagement.launcherPath}\cachedToken.txt";

        if (!File.Exists(path)) File.WriteAllText(path, string.Empty);
        Trace.Listeners.Add(new AutoFlushTextWriterTraceListener(File.Create($@"{info.FullName}\{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt")));

        Config.LoadConfig();
    }
}