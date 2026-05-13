using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;
using SkiaSharp;

namespace Flarial.Launcher.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    const int NativeCornerRadius = 25;
    const int DwmwaWindowCornerPreference = 33;
    const int DwmwaBorderColor = 34;
    const int DwmwcpRound = 2;
    const uint DwmColorNone = 0xFFFFFFFE;

    public static Canvas? ToolTipLayerInstance { get; private set; }
    
    public MainWindow()
    {
        InitializeComponent();
        
        ToolTipLayerInstance = ToolTipLayer;
        
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        
        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Minimize)
            .Subscribe(_ => WindowState = WindowState.Minimized);
        
        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Close)
            .Subscribe(_ => Close());
        
        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyRoundedWindowRegion();

        if (DataContext is not MainWindowViewModel vm) return;
        //await Task.Delay(200);
        await vm.InitializeSettingsAsync();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel { IsVersionInstallActive: true } vm)
        {
            e.Cancel = true;
            vm.NotifyInstallCloseBlocked();
            return;
        }

        base.OnClosing(e);
    }

    void ApplyRoundedWindowRegion()
    {
        var handle = TryGetPlatformHandle()?.Handle ?? 0;
        if (handle == 0) return;

        DisableNativeWindowBorder(handle);

        var scale = RenderScaling;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));
        var radius = Math.Max(1, (int)Math.Round(NativeCornerRadius * scale));

        var region = CreateRoundRectRgn(0, 0, width + 1, height + 1, radius * 2, radius * 2);
        if (region == 0) return;

        if (SetWindowRgn(handle, region, true) == 0)
            DeleteObject(region);
    }

    static void DisableNativeWindowBorder(nint handle)
    {
        var cornerPreference = DwmwcpRound;
        DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));

        var borderColor = DwmColorNone;
        DwmSetWindowAttribute(handle, DwmwaBorderColor, ref borderColor, sizeof(uint));
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

    [DllImport("gdi32.dll")]
    static extern nint CreateRoundRectRgn(int left, int top, int right, int bottom, int ellipseWidth, int ellipseHeight);

    [DllImport("user32.dll")]
    static extern int SetWindowRgn(nint hwnd, nint region, bool redraw);

    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(nint objectHandle);

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int attributeValue, int attributeSize);

    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref uint attributeValue, int attributeSize);
}
