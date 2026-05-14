using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Flarial.Launcher.Types;
using Flarial.Launcher.ViewModels;
using ReactiveUI;

namespace Flarial.Launcher.Views;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MainWindow : Window
{
    const int NativeCornerRadius = 25;
    const int DwmwaNcRenderingPolicy = 2;
    const int DwmwaWindowCornerPreference = 33;
    const int DwmwaBorderColor = 34;
    const int DwmncrpDisabled = 1;
    const int DwmwcpRound = 2;
    const uint DwmColorNone = 0xFFFFFFFE;

    public static Canvas? ToolTipLayerInstance { get; private set; }

    WindowEdge? _resizeEdge;
    Point _resizeStartPoint;
    PixelPoint _resizeStartPosition;
    double _resizeStartWidth;
    double _resizeStartHeight;
    
    public MainWindow()
    {
        InitializeComponent();
        
        ToolTipLayerInstance = ToolTipLayer;
        
        SystemDecorations = SystemDecorations.None;
        
        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Minimize)
            .Subscribe(_ => WindowState = WindowState.Minimized);
        
        MessageBus.Current.Listen<WindowStateArgs>()
            .Where(e => e == WindowStateArgs.Close)
            .Subscribe(_ => Close());
        
        MessageBus.Current.Listen<PageTransitions>()
            .Subscribe(PageTransition);

    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
            ApplyRoundedWindowRegion();
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        TerminateProcess(GetCurrentProcess(), 0);
    }

    void ApplyRoundedWindowRegion()
    {
        var handle = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero) return;

        DisableNativeWindowBorder(handle);

        var scale = RenderScaling;
        var launcherWidth = ScaleToPixels(Math.Max(MinWidth, Bounds.Width), scale);
        var launcherHeight = ScaleToPixels(Math.Max(MinHeight, Bounds.Height), scale);
        var launcherRadius = ScaleToPixels(NativeCornerRadius, scale) * 2;
        var region = CreateRoundRectRgn(0, 0, launcherWidth + 1, launcherHeight + 1, launcherRadius, launcherRadius);
        if (region == 0) return;

        if (SetWindowRgn(handle, region, true) == 0)
            DeleteObject(region);
    }

    static int ScaleToPixels(double value, double scale) => Math.Max(1, (int)Math.Round(value * scale));

    static void DisableNativeWindowBorder(nint handle)
    {
        var nonClientRenderingPolicy = DwmncrpDisabled;
        DwmSetWindowAttribute(handle, DwmwaNcRenderingPolicy, ref nonClientRenderingPolicy, sizeof(int));

        var cornerPreference = DwmwcpRound;
        DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));

        var borderColor = DwmColorNone;
        DwmSetWindowAttribute(handle, DwmwaBorderColor, ref borderColor, sizeof(uint));
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);

    private void ResizeWindow(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var edge = sender switch
        {
            Control { Name: "ResizeNorth" } => WindowEdge.North,
            Control { Name: "ResizeSouth" } => WindowEdge.South,
            Control { Name: "ResizeWest" } => WindowEdge.West,
            Control { Name: "ResizeEast" } => WindowEdge.East,
            Control { Name: "ResizeNorthWest" } => WindowEdge.NorthWest,
            Control { Name: "ResizeNorthEast" } => WindowEdge.NorthEast,
            Control { Name: "ResizeSouthWest" } => WindowEdge.SouthWest,
            Control { Name: "ResizeSouthEast" } => WindowEdge.SouthEast,
            _ => WindowEdge.SouthEast
        };

        _resizeEdge = edge;
        _resizeStartPoint = e.GetPosition(this);
        _resizeStartPosition = Position;
        _resizeStartWidth = Width;
        _resizeStartHeight = Height;

        control.PointerMoved += ResizeHandle_OnPointerMoved;
        control.PointerReleased += ResizeHandle_OnPointerReleased;
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private void ResizeHandle_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_resizeEdge is not { } edge || sender is not Control control)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            EndResize(control, e.Pointer);
            return;
        }

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _resizeStartPoint.X;
        var deltaY = currentPoint.Y - _resizeStartPoint.Y;
        var newWidth = _resizeStartWidth;
        var newHeight = _resizeStartHeight;
        var newX = _resizeStartPosition.X;
        var newY = _resizeStartPosition.Y;

        if (edge is WindowEdge.East or WindowEdge.NorthEast or WindowEdge.SouthEast)
            newWidth = Math.Max(MinWidth, _resizeStartWidth + deltaX);

        if (edge is WindowEdge.South or WindowEdge.SouthEast or WindowEdge.SouthWest)
            newHeight = Math.Max(MinHeight, _resizeStartHeight + deltaY);

        if (edge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest)
        {
            newWidth = Math.Max(MinWidth, _resizeStartWidth - deltaX);
            newX = _resizeStartPosition.X + ScaleToPixels(_resizeStartWidth - newWidth, RenderScaling);
        }

        if (edge is WindowEdge.North or WindowEdge.NorthEast or WindowEdge.NorthWest)
        {
            newHeight = Math.Max(MinHeight, _resizeStartHeight - deltaY);
            newY = _resizeStartPosition.Y + ScaleToPixels(_resizeStartHeight - newHeight, RenderScaling);
        }

        Width = newWidth;
        Height = newHeight;
        Position = new PixelPoint(newX, newY);
        e.Handled = true;
    }

    private void ResizeHandle_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control control)
            EndResize(control, e.Pointer);

        e.Handled = true;
    }

    private void EndResize(Control control, IPointer pointer)
    {
        _resizeEdge = null;
        control.PointerMoved -= ResizeHandle_OnPointerMoved;
        control.PointerReleased -= ResizeHandle_OnPointerReleased;
        pointer.Capture(null);
    }

    private async void PageTransition(PageTransitions page)
    {
        if (DataContext is not MainWindowViewModel vm) return;
        vm.IsAnimating = true;

        var tasks = new List<Task>();
        var pageHeight = Math.Max(1, Bounds.Height);

        switch (page)
        {
            case PageTransitions.SettingsPage:
                SettingsViewControl.IsHitTestVisible = true;
                SettingsViewControl.Opacity = 1;
                tasks.Add(CreateHomeTransition(0, -pageHeight, 1, 1, 0.9625, 0.94).RunAsync(HomeViewControl));
                tasks.Add(CreateSettingsTransition(pageHeight, 0).RunAsync(SettingsViewControl));
                break;

            case PageTransitions.HomePage:
                HomeViewControl.IsVisible = true;
                tasks.Add(CreateHomeTransition(-pageHeight, 0, 0.9625, 0.94, 1, 1).RunAsync(HomeViewControl));
                tasks.Add(CreateSettingsTransition(0, pageHeight).RunAsync(SettingsViewControl));
                break;

            case PageTransitions.SettingsGeneralPage:
            case PageTransitions.SettingsVersionsPage:
            case PageTransitions.SettingsConfigsPage:
            default:
                vm.IsAnimating = false;
                return;
        }

        await Task.WhenAll(tasks);

        if (page == PageTransitions.SettingsPage)
            HomeViewControl.IsVisible = false;
        else if (page == PageTransitions.HomePage)
        {
            SettingsViewControl.IsHitTestVisible = false;
            SettingsViewControl.Opacity = 0;
        }
        
        vm.IsAnimating = false;
    }

    static Animation CreateHomeTransition(
        double fromY,
        double toY,
        double fromScaleX,
        double fromScaleY,
        double toScaleX,
        double toScaleY) => new()
    {
        Duration = TimeSpan.FromMilliseconds(500),
        Easing = new QuadraticEaseInOut(),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0),
                Setters =
                {
                    new Setter(ScaleTransform.ScaleXProperty, fromScaleX),
                    new Setter(ScaleTransform.ScaleYProperty, fromScaleY),
                    new Setter(TranslateTransform.YProperty, fromY)
                }
            },
            new KeyFrame
            {
                Cue = new Cue(0.45),
                Setters =
                {
                    new Setter(ScaleTransform.ScaleXProperty, 0.9625),
                    new Setter(ScaleTransform.ScaleYProperty, 0.94),
                    new Setter(TranslateTransform.YProperty, 0)
                }
            },
            new KeyFrame
            {
                Cue = new Cue(0.55),
                Setters =
                {
                    new Setter(ScaleTransform.ScaleXProperty, 0.9625),
                    new Setter(ScaleTransform.ScaleYProperty, 0.94),
                    new Setter(TranslateTransform.YProperty, 0)
                }
            },
            new KeyFrame
            {
                Cue = new Cue(1),
                Setters =
                {
                    new Setter(ScaleTransform.ScaleXProperty, toScaleX),
                    new Setter(ScaleTransform.ScaleYProperty, toScaleY),
                    new Setter(TranslateTransform.YProperty, toY)
                }
            }
        }
    };

    static Animation CreateSettingsTransition(double fromY, double toY) => new()
    {
        Duration = TimeSpan.FromMilliseconds(500),
        Easing = new QuadraticEaseInOut(),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0),
                Setters = { new Setter(TranslateTransform.YProperty, fromY) }
            },
            new KeyFrame
            {
                Cue = new Cue(0.45),
                Setters = { new Setter(TranslateTransform.YProperty, fromY) }
            },
            new KeyFrame
            {
                Cue = new Cue(0.55),
                Setters = { new Setter(TranslateTransform.YProperty, fromY) }
            },
            new KeyFrame
            {
                Cue = new Cue(1),
                Setters = { new Setter(TranslateTransform.YProperty, toY) }
            }
        }
    };

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
