using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Flarial.Launcher.Management;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

public partial class App : Application
{
    readonly AppSettings _appSettings = AppSettings.Get();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;

        lifetime.Exit += OnExit;
        lifetime.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(_appSettings) };

        base.OnFrameworkInitializationCompleted();
    }

    void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args) => _appSettings.Set();
}