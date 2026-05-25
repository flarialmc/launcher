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

    public MainWindow()
    {
        InitializeComponent();

        ToolTipLayerInstance = ToolTipLayer;

        SystemDecorations = SystemDecorations.None;
        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        //ExtendClientAreaTitleBarHeightHint = -1;

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Minimize)
            .Subscribe(_ => WindowState = WindowState.Minimized);

        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Close)
            .Subscribe(_ => Close());

        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);

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
                homeViewAnimation = (Animation?)Application.Current?.Resources["HomePageLeaveTransition"];
                settingsViewAnimation = (Animation?)Application.Current?.Resources["SettingsPageEnterTransition"];
                break;

            case PageTransitions.HomePage:
                homeViewAnimation = (Animation?)Application.Current?.Resources["HomePageEnterTransition"];
                settingsViewAnimation = (Animation?)Application.Current?.Resources["SettingsPageLeaveTransition"];
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

    async void OnLoaded(object sender, RoutedEventArgs args)
    {
        Loaded -= OnLoaded;
        ((MainWindowViewModel)DataContext!).OnLoaded();
    }

}