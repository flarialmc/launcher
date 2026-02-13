using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

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
        System.Windows.MessageBox.Show(text, "Flarial Version Changer: Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

        Environment.Exit(1);
    };

    [STAThread]
    static void Main()
    {
        using (WindowsXamlManager.InitializeForCurrentThread())
        {
            var application = Application.Current;
            ColorPaletteResources resources = new() { Accent = Colors.IndianRed };

            application.RequestedTheme = ApplicationTheme.Dark;
            application.Resources.MergedDictionaries.Add(resources);

            new System.Windows.Application().Run(new MainWindow());
        }
    }
}