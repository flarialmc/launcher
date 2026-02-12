using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using ModernWpf;
using ModernWpf.Controls;

static class Program
{
    const string Format = @"Looks like the application crashed! 

• Please take a screenshot of this.
• Create a new support post & send the screenshot.

Exception: {0}

{1}

{2}";

    static Program() => AppDomain.CurrentDomain.UnhandledException += static (sender, args) =>
    {
        var exception = (Exception)args.ExceptionObject;
        var trace = exception.StackTrace.Trim();

        while (exception.InnerException is not null)
            exception = exception.InnerException;

        var name = exception.GetType().Name;
        var message = exception.Message;

        var text = string.Format(Format, name, message, trace);
        MessageBox.Show(text, "Flarial Version Changer: Error", MessageBoxButton.OK, MessageBoxImage.Error);

        Environment.Exit(1);
    };


    [STAThread]
    static void Main()
    {
        using var _ = new Mutex(default, "F692A90B-7CD3-4D02-8A19-38E31C769CC0", out var created);
        if (!created) return;

        Application application = new();
        application.Resources.MergedDictionaries.Add(new ThemeResources());
        application.Resources.MergedDictionaries.Add(new XamlControlsResources());
        application.Resources.MergedDictionaries.Add(new ColorPaletteResources { Accent = Colors.IndianRed });
        application.Run(new MainWindow());
    }
}