using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Avalonia.Threading;
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
        
        SystemDecorations = SystemDecorations.None;
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
        await WarmUpTransitionViewsAsync();
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        TerminateProcess(GetCurrentProcess(), 0);
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
        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero) return;

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

            using var stream = await response.Content.ReadAsStreamAsync();
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;

            Bitmap bitmap;
            try
            {
                bitmap = CreatePromotionBitmap(buffer);
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

    static Bitmap CreatePromotionBitmap(Stream stream)
    {
        using var skStream = new SKManagedStream(stream);
        using var source = SKBitmap.Decode(skStream) ?? throw new InvalidDataException("Unable to decode promotion image.");
        using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);
        canvas.DrawBitmap(source, 0, 0);
        canvas.Flush();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new Bitmap(data.AsStream());
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

    async Task WarmUpTransitionViewsAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            WarmUpView(HomeViewControl);
            WarmUpView(SettingsViewControl);
            WarmUpTransitionTransforms();
        }, DispatcherPriority.Background);
    }

    void WarmUpView(Control view)
    {
        if (view.Bounds.Width <= 0 || view.Bounds.Height <= 0) return;

        var scale = RenderScaling;
        var pixelSize = new PixelSize(
            ScaleToPixels(view.Bounds.Width, scale),
            ScaleToPixels(view.Bounds.Height, scale));

        using var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96 * scale, 96 * scale));
        bitmap.Render(view);
    }

    void WarmUpTransitionTransforms()
    {
        if (HomeViewControl.RenderTransform is not TransformGroup homeTransforms)
            return;

        var homeScale = homeTransforms.Children.OfType<ScaleTransform>().FirstOrDefault();
        var homeTranslate = homeTransforms.Children.OfType<TranslateTransform>().FirstOrDefault();
        if (homeScale is null || homeTranslate is null)
            return;

        var originalScaleX = homeScale.ScaleX;
        var originalScaleY = homeScale.ScaleY;
        var originalHomeY = homeTranslate.Y;

        homeScale.ScaleX = 0.9625;
        homeScale.ScaleY = 0.94;
        homeTranslate.Y = -500;
        WarmUpView(HomeViewControl);

        if (SettingsViewControl.RenderTransform is TranslateTransform settingsTranslate)
        {
            var originalSettingsY = settingsTranslate.Y;
            settingsTranslate.Y = 500;
            SettingsViewControl.Opacity = 1;
            WarmUpView(SettingsViewControl);
            settingsTranslate.Y = originalSettingsY;
            SettingsViewControl.Opacity = 0;
        }

        homeScale.ScaleX = originalScaleX;
        homeScale.ScaleY = originalScaleY;
        homeTranslate.Y = originalHomeY;
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

    [DllImport("kernel32.dll")]
    static extern nint GetCurrentProcess();

    [DllImport("kernel32.dll")]
    static extern bool TerminateProcess(nint processHandle, uint exitCode);
}
