using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Flarial.Runtime.Unmanaged;

[assembly: AssemblyCompany("Flarial")]
[assembly: AssemblyProduct("Launcher")]
[assembly: AssemblyTitle("Flarial Launcher")]
[assembly: SupportedOSPlatform("windows10.0.19041.0")]
[assembly: AssemblyCopyright("Copyright © Flarial 2023 - 2026")]

file static class AssemblyInfo
{
    const string Format = @"Looks like the launcher crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}";

    [ModuleInitializer]
    internal static void ModuleInitializer() => AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

    static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = $"{assembly.GetName().Version}";

        var exception = (Exception)args.ExceptionObject;
        while (exception.InnerException is not null) exception = exception.InnerException;

        var message = exception.Message;
        var name = exception.GetType().Name;

#if DEBUG
        message = $"{args.ExceptionObject}";
#endif

        nint handle = 0;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            if (lifetime.MainWindow is not { } window) return;
            handle = new(window.TryGetPlatformHandle()?.Handle ?? 0);
        }

        var text = string.Format(Format, version, name, message);
        NativeMethods.MessageBox(handle, text, "Flarial Launcher: Error");

        Environment.Exit(1);
    }

}