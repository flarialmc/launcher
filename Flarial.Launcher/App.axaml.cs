using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flarial.Launcher.Management;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

public partial class App : Application
{
    readonly AppSettings _settings = AppSettings.Get();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_settings),
            };

            desktop.Exit += (_, _) => _settings.Set();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
