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
    const string Format = @"• Please take a screenshot of this.
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
        var information = exception.StackTrace?.Trim();
        while (exception.InnerException is { }) exception = exception.InnerException;

        var message = exception.Message;
        var type = exception.GetType().Name;

        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        nint handle = new(lifetime?.MainWindow?.TryGetPlatformHandle()?.Handle ?? 0);

        new NativeDialog
        {
            Handle = handle,

            Title = "Flarial Launcher: Error",
            Content = string.Format(Format, version, type, message),

            Information = information,
            Instruction = "Looks like the launcher crashed!",
        }.Show();

        Environment.Exit(1);
    }
}