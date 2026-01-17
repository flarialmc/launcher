using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Flarial.Launcher.Interface;
using Flarial.Launcher.Management;
using Flarial.Launcher.Services.Modding;
using Flarial.Launcher.Services.Networking;
using ModernWpf;
using ModernWpf.Controls;
using static System.IO.Directory;
using static System.Environment;
using static System.Environment.SpecialFolder;
using static Flarial.Launcher.PInvoke;
using static System.IO.Path;
using Flarial.Launcher.Services.Core;
using System.Threading.Tasks;
using System.IO;

namespace Flarial.Launcher;

sealed class Program : Application
{
    const string Format = @"Looks like the launcher crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Version: {0}
Exception: {1}

{2}

{3}";

    static Program()
    {
        /*
            - Prevent the operating system from handling errors for us.
            - Setup exception & process exit handlers.
        */

        SetErrorMode(SEM_NOGPFAULTERRORBOX | SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX | SEM_NOALIGNMENTFAULTEXCEPT);

        AppDomain.CurrentDomain.ProcessExit += static (_, _) => { GC.Collect(); GC.WaitForPendingFinalizers(); };

        AppDomain.CurrentDomain.UnhandledException += static (sender, args) =>
        {
            var version = ApplicationManifest.Version;

            var exception = (Exception)args.ExceptionObject;
            var trace = $"{exception.StackTrace}".Trim();

            while (exception.InnerException is not null)
                exception = exception.InnerException;

            var name = exception.GetType().Name;
            var message = exception.Message;

            var text = string.Format(Format, version, name, message, trace);
            MessageBox.Show(text, "Flarial Launcher: Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(1);
        };
    }

    [STAThread]
    static void Main(string[] args)
    {
        using var _ = new Mutex(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created);
        if (!created)
            return;

        CurrentDirectory = CreateDirectory(Combine(GetFolderPath(LocalApplicationData), @"Flarial\Launcher")).FullName;

        var configuration = Configuration.Get();

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--inject":
                    if (!(index + 1 < args.Length))
                        continue;

                    Injector.Launch(true, new(args[index + 1]));
                    return;

                case "--use-proxy":
                    HttpService.UseProxy = true; break;

                case "--use-dns-over-https":
                    HttpService.UseDnsOverHttps = true;
                    break;

                case "--no-hardware-acceleration":
                    configuration.HardwareAcceleration = false;
                    break;

                case "--allow-unsigned-installs":
                    Minecraft.AllowUnsignedInstalls = true;
                    break;
            }
        }

        /*
            - Preload sponsorship banner into memory.
            - This should speedup rendering the banner.
        */

        new Program(configuration).Run(new MainWindow(configuration, Sponsorship.BannerAsync()));
    }

    readonly Configuration _configuration;

    Program(Configuration configuration)
    {
        _configuration = configuration;
        Resources.MergedDictionaries.Add(new ThemeResources());
        Resources.MergedDictionaries.Add(new XamlControlsResources());
        Resources.MergedDictionaries.Add(new ColorPaletteResources { Accent = Colors.IndianRed });
    }

    protected override void OnExit(ExitEventArgs args) { base.OnExit(args); _configuration.Save(); }
}