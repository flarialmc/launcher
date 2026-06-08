using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flarial.Launcher.Management;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

public partial class App : Application
{
    public AppSettings AppSettings { get; } = AppSettings.Get();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;

        lifetime.Exit += (_, _) => AppSettings.Set();
        lifetime.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(AppSettings) };

        base.OnFrameworkInitializationCompleted();
    }
}