using System;
using System.IO;
using System.Linq;
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

namespace Flarial.Launcher;

static class Program
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

            Exit(1);
        };
    }

    [STAThread]
    static void Main(string[] args)
    {
        using (new Mutex(default, "54874D29-646C-4536-B6D1-8E05053BE00E", out var created))
        {
            if (!created) return;

            var path = GetFolderPath(LocalApplicationData);
            path = Path.Combine(path, @"Flarial\Launcher");
            CurrentDirectory = CreateDirectory(path).FullName;

            var configuration = Configuration.Get();

            for (var index = 0; index < args.Length; index++)
            {
                var argument = args[index];
                switch (argument)
                {
                    case "--inject":
                        if (!(index + 1 < args.Length)) continue;
                        Injector.Launch(true, new(args[index + 1]));
                        return;

                    case "--use-proxy": HttpService.UseProxy = true; break;
                    case "--use-dns-over-https": HttpService.UseDnsOverHttps = true; break;
                    case "--no-hardware-acceleration": configuration.HardwareAcceleration = false; break;
                }
            }

            Application application = new();

            application.Resources.MergedDictionaries.Add(new ThemeResources());
            application.Resources.MergedDictionaries.Add(new XamlControlsResources());
            application.Resources.MergedDictionaries.Add(new ColorPaletteResources { Accent = Colors.IndianRed });

            application.Exit += (_, _) => configuration.Save();
            application.Run(new MainWindow(configuration));
        }
    }
}