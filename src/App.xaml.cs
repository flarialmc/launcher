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
    static readonly Mutex Mutex;

    static App()
    {
        /*
            - Do not shift or alter this handler since it catches, handles & displays errors.
            - Start writing code below this handler to ensure exceptions are handled.
        */

        AppDomain.CurrentDomain.UnhandledException +=
        [SecurityCritical, HandleProcessCorruptedStateExceptions] (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;

            var @this = exception;
            while (@this.InnerException is not null) @this = @this.InnerException;

            string message = $"Version: {Assembly.GetEntryAssembly().GetName().Version}\nException: {@this.GetType().Name}\n\n{@this.Message}\n\n{exception.StackTrace.Trim()}";

            Trace.WriteLine(message);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(default);
        };

        var args = Environment.GetCommandLineArgs();
        for (var index = 0; index + 1 < args.Length; index++)
            switch (args[index])
            {
                case "inject":
                    Loader.Launch(new ArraySegment<string>(args, ++index, args.Length - index));
                    Environment.Exit(default);
                    break;
            }

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Mutex = new(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var value);
        if (!value) Environment.Exit(default);

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