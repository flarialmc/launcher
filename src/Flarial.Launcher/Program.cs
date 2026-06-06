using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Flarial.Runtime.Game;
using ReactiveUI.Avalonia;

namespace Flarial.Launcher;

static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using Mutex mutex = new(false, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created) return;

        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Environment.CurrentDirectory = Directory.CreateDirectory(Path.Combine(path, @"Flarial\Launcher")).FullName;

        for (var index = 0; index < args.Length; index++)
            switch (args[index])
            {
                case "--inject":
                    if (!(index + 1 < args.Length)) continue;
                    Injector.Launch(false, new(args[index + 1]));
                    return;
            }

        var builder = AppBuilder.Configure<App>();

        builder.UseSkia();
        builder.UseWin32();
        builder.UseHarfBuzz();
        builder.UseReactiveUI(static _ => _.WithExceptionHandler(new ExceptionHandler()));

        builder.With(new CompositionOptions { UseRegionDirtyRectClipping = true });
        builder.With(new SkiaOptions { MaxGpuResourceSizeBytes = long.MaxValue });
        builder.With(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None });
        builder.With(new Win32PlatformOptions { CompositionMode = [Win32CompositionMode.WinUIComposition] });

        builder.StartWithClassicDesktopLifetime(args);
    }
}