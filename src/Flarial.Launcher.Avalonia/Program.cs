using System;
using System.IO;
using System.Threading;
using Avalonia;
using ReactiveUI.Avalonia;
using static System.Environment;
using static System.Environment.SpecialFolder;


namespace Flarial.Launcher;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using Mutex mutex = new(false, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created) return;

        var path = Path.Combine(GetFolderPath(LocalApplicationData), @"Flarial\Launcher");
        CurrentDirectory = Directory.CreateDirectory(path).FullName;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
    }
}