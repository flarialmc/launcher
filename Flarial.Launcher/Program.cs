using System;
using System.IO;
using System.Threading;
using Avalonia;
using Flarial.Runtime.Modding;
using ReactiveUI.Avalonia;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Flarial.Launcher;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using Mutex mutex = new(false, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created) return;

        var path = Path.Combine(GetFolderPath(LocalApplicationData), @"Flarial\Launcher");
        CurrentDirectory = Directory.CreateDirectory(path).FullName;

        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] != "--inject" || index + 1 >= args.Length) continue;

            Injector.Launch(new(args[index + 1]));
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
