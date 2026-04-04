using System;
using System.IO;
using System.Threading;
using System.Windows;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Management;
using Flarial.Runtime.Modding;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Diagnostics.Debug.THREAD_ERROR_MODE;

namespace Flarial.Launcher;

sealed class App : System.Windows.Application
{
    const string Format = @"Looks like the launcher crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}

{3}";

    static App()
    {
        SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX | SEM_NOALIGNMENTFAULTEXCEPT);
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    static void OnUnhandledException(Exception exception)
    {
        var trace = exception.StackTrace.Trim();

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var name = exception.GetType().Name;
        var message = exception.Message;

        var text = string.Format(Format, AppManifest.s_version, name, message, trace);
        MessageBox.Show(text, "Flarial Launcher: Error", MessageBoxButton.OK, MessageBoxImage.Error);

        Environment.Exit(1);
    }

    static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs args) => OnUnhandledException((Exception)args.ExceptionObject);

    static void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs args) { args.Handled = true; OnUnhandledException(args.Exception); }

    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(false, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created) return;

        var path = Path.Combine(GetFolderPath(LocalApplicationData), @"Flarial\Launcher");
        CurrentDirectory = Directory.CreateDirectory(path).FullName;

        for (var index = 0; index < args.Length; index++)
            switch (args[index])
            {
                case "--inject":
                    if (!(index + 1 < args.Length)) continue;
                    Injector.Launch(true, new(args[index + 1]));
                    return;
            }

        using (WindowsXamlManager.InitializeForCurrentThread())
        {
            var application = global::Windows.UI.Xaml.Application.Current;
            application.UnhandledException += OnUnhandledException;

            application.RequestedTheme = ApplicationTheme.Dark;
            application.Resources.MergedDictionaries.Add(new ColorPaletteResources { Accent = Colors.IndianRed });

            var settings = AppSettings.Get();
            new App(settings).Run(new HostWindow(settings));
        }
    }

    readonly AppSettings _settings;

    App(AppSettings settings) => _settings = settings;

    protected override void OnExit(ExitEventArgs args)
    {
        base.OnExit(args);
        _settings.Flush();
    }
}