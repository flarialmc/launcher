using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flarial.Launcher.Management;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

public sealed partial class App : Application
{
    public AppSettings Settings { get; } = AppSettings.Get();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var lifetime = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;

        lifetime.Exit += (_, _) => Settings.Set();
        lifetime.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

        base.OnFrameworkInitializationCompleted();
    }
}