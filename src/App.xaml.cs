using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Bedrockix.Minecraft;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher;

public partial class App : Application
{
#pragma warning disable IDE0052
    static readonly Mutex Mutex;
#pragma warning restore IDE0052

    static App()
    {

        if (Environment.GetCommandLineArgs().Length > 1)
        {
            string arg = Environment.GetCommandLineArgs()[1];
            if (arg == "inject")
            {
                string arg2 = Environment.GetCommandLineArgs()[2];
                Loader.Launch(arg2);
            }
            Environment.Exit(1);
        }
        
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var exception = (Exception)args.ExceptionObject;

            var @this = exception;
            while (@this.InnerException is not null) @this = @this.InnerException;

            string message = $"Version: {Assembly.GetEntryAssembly().GetName().Version}\nException: {@this.GetType().Name}\n\n{@this.Message}\n\n{exception.StackTrace.Trim()}";

            Trace.WriteLine(message);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(default);
        };

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
        
        
    }
}