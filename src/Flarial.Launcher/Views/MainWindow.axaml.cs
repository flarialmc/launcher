using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Flarial.Launcher.Management;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;
using SkiaSharp;
using Windows.Win32;

namespace Flarial.Launcher.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    public static Canvas? ToolTipLayerInstance { get; private set; }

    readonly App _application;
    readonly AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();

        _application = (App)Application.Current!;
        _settings = _application.Settings;

        ToolTipLayerInstance = ToolTipLayer;
        WindowDecorations = WindowDecorations.None;
        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaTitleBarHeightHint = -1;

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(static e => e == WindowStateArgs.Minimize)
            .Subscribe(_ => WindowState = WindowState.Minimized);

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(static e => e == WindowStateArgs.Close)
            .Subscribe(_ => Close());

        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);
    }

    void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        try
        {
            if (sender is not Control control)
                return;

            var point = args.GetCurrentPoint(control);

            if (point.Properties.IsLeftButtonPressed)
                BeginMoveDrag(args);
        }
        catch { }
    }

    private async void PageTransition(PageTransitions page)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.IsAnimating = true;

        var tasks = new List<Task>();
        Animation? homeViewAnimation;
        Animation? settingsViewAnimation;

        switch (page)
        {
            case PageTransitions.SettingsPage:
                if (_settings.PerformanceMode)
                {
                    homeViewAnimation = (Animation?)_application.Resources["PerformanceHomePageLeaveTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["PerformanceSettingsPageEnterTransition"];
                }
                else
                {
                    homeViewAnimation = (Animation?)_application.Resources["HomePageLeaveTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["SettingsPageEnterTransition"];
                }
                break;
            case PageTransitions.HomePage:
                if (_settings.PerformanceMode)
                {
                    homeViewAnimation = (Animation?)_application.Resources["PerformanceHomePageEnterTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["PerformanceSettingsPageLeaveTransition"];
                }
                else
                {
                    homeViewAnimation = (Animation?)_application.Resources["HomePageEnterTransition"];
                    settingsViewAnimation = (Animation?)_application.Resources["SettingsPageLeaveTransition"];
                }
                break;
            case PageTransitions.SettingsGeneralPage:
            case PageTransitions.SettingsVersionsPage:
            case PageTransitions.SettingsConfigsPage:
            default:
                vm.IsAnimating = false;
                return;
        }

        if (homeViewAnimation is not null)
            tasks.Add(homeViewAnimation.RunAsync(HomeViewControl));

        if (settingsViewAnimation is not null)
            tasks.Add(settingsViewAnimation.RunAsync(SettingsViewControl));

        await Task.WhenAll(tasks);

        vm.IsAnimating = false;
    }

    async void OnLoaded(object? sender, RoutedEventArgs args)
    {
        Loaded -= OnLoaded;
        ((MainWindowViewModel)DataContext!).OnLoaded();
    }

}