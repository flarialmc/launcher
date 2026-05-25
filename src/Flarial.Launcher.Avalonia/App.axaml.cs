using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flarial.Launcher.ViewModels;
using Flarial.Launcher.Views;

namespace Flarial.Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime) return;

        lifetime.MainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel()
        };

        base.OnFrameworkInitializationCompleted();
    }
}