using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Flarial.Runtime.Modding;
using ReactiveUI.Avalonia;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.Debug.THREAD_ERROR_MODE;

namespace Flarial.Launcher;

static class Program
{
    const string Format = @"Looks like the launcher crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}";

    static Program()
    {
        SetErrorMode(SEM_FAILCRITICALERRORS);
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    unsafe static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = $"{assembly.GetName().Version}";

        var exception = (Exception)args.ExceptionObject;

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var message = exception.Message;
        var name = exception.GetType().Name;

        fixed (char* caption = "Flarial Launcher: Error")
        fixed (char* text = string.Format(Format, version, name, message, string.Empty))
        {
            var application = Application.Current;
            var lifetime = (IClassicDesktopStyleApplicationLifetime?)application?.ApplicationLifetime;

            var window = lifetime?.MainWindow;
            var handle = window?.TryGetPlatformHandle()?.Handle;

            HWND hWnd = new(handle ?? new());
            MessageBox(hWnd, text, caption, MESSAGEBOX_STYLE.MB_ICONERROR);
        }

        Environment.Exit(1);
    }

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
                    Injector.Launch(new(args[index + 1]));
                    return;
            }

        var builder = AppBuilder.Configure<App>();

        builder.UseSkia();
        builder.UseWin32();
        builder.UseHarfBuzz();
        builder.UseReactiveUI(static _ => { });

        builder.With(new CompositionOptions { UseRegionDirtyRectClipping = true });
        builder.With(new SkiaOptions { MaxGpuResourceSizeBytes = long.MaxValue });
        builder.With(new RenderOptions { BitmapInterpolationMode = BitmapInterpolationMode.None });
        builder.With(new Win32PlatformOptions { CompositionMode = [Win32CompositionMode.WinUIComposition] });

        builder.StartWithClassicDesktopLifetime(args);
    }
}