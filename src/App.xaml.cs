using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Flarial.Launcher.Managers;

namespace Flarial.Launcher;

public partial class App : Application
{
#pragma warning disable IDE0052
    static readonly Mutex Mutex;
#pragma warning restore IDE0052

    static App()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Mutex = new(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var value);
        if (!value) Environment.Exit(default);

        var info = Directory.CreateDirectory(@$"{VersionManagement.launcherPath}\Logs");
        var stream = File.Create($@"{info.FullName}\{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt");
        Trace.Listeners.Add(new AutoFlushTextWriterTraceListener(stream));
    }
}