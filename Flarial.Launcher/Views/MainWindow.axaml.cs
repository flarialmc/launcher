using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using Flarial.Runtime.Services;
using ReactiveUI;
using SkiaSharp;

namespace Flarial.Launcher.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    const int NativeCornerRadius = 25;
    const int LauncherWidth = 800;
    const int LauncherHeight = 500;
    const int AdWidth = 320;
    const int DwmwaWindowCornerPreference = 33;
    const int DwmwaBorderColor = 34;
    const int DwmwcpRound = 2;
    const uint DwmColorNone = 0xFFFFFFFE;

    static readonly HttpClient s_httpClient = new();
    Uri? _adUri;
    bool _promotionLoaded;

    public static Canvas? ToolTipLayerInstance { get; private set; }
    
    public MainWindow()
    {
        InitializeComponent();
        
        ToolTipLayerInstance = ToolTipLayer;
        
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        
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
        _ = ShowPromotionAfterDelayAsync();
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

    void OnActivated(object? sender, EventArgs e)
    {
        if (_promotionLoaded)
            AdPopup.IsOpen = true;
    }

    void OnDeactivated(object? sender, EventArgs e)
    {
        AdPopup.IsOpen = false;
    }

    void ApplyRoundedWindowRegion()
    {
        var handle = TryGetPlatformHandle()?.Handle ?? 0;
        if (handle == 0) return;

        DisableNativeWindowBorder(handle);

        var scale = RenderScaling;
        var launcherWidth = ScaleToPixels(LauncherWidth, scale);
        var launcherHeight = ScaleToPixels(LauncherHeight, scale);
        var launcherRadius = ScaleToPixels(NativeCornerRadius, scale) * 2;
        var region = CreateRoundRectRgn(0, 0, launcherWidth + 1, launcherHeight + 1, launcherRadius, launcherRadius);
        if (region == 0) return;

        if (SetWindowRgn(handle, region, true) == 0)
            DeleteObject(region);
    }

    static int ScaleToPixels(double value, double scale) => Math.Max(1, (int)Math.Round(value * scale));

    static void DisableNativeWindowBorder(nint handle)
    {
        var cornerPreference = DwmwcpRound;
        DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));

        var borderColor = DwmColorNone;
        DwmSetWindowAttribute(handle, DwmwaBorderColor, ref borderColor, sizeof(uint));
    }

    async Task ShowPromotionAfterDelayAsync()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500));

        try
        {
            var promotions = await PromotionManager.GetAsync();
            if (promotions.Length == 0) return;

            var promotion = promotions[0];
            if (!Uri.TryCreate(promotion.Uri, UriKind.Absolute, out _adUri)
                || !Uri.TryCreate(promotion.Image, UriKind.Absolute, out var imageUri))
            {
                return;
            }

            using var response = await s_httpClient.GetAsync(imageUri);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentType?.MediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) != true)
                return;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;

            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(buffer);
            }
            catch
            {
                return;
            }

            AdImage.Source = bitmap;
            _promotionLoaded = true;
            AdPopup.IsOpen = IsActive;

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(350),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters = { new Setter(TranslateTransform.YProperty, 0d) }
                    }
                }
            };

            await animation.RunAsync(AdBorder);
        }
        catch
        {
            // Promotions are optional; ad failures should never interrupt the launcher.
        }
    }
    
    private void DragWindow(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);

    void AdBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_adUri is null) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = _adUri.ToString(),
            UseShellExecute = true
        });
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
