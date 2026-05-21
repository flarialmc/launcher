using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Flarial.Launcher.Views;

namespace Flarial.Launcher.Controls.ToolTip;

public abstract class CustomToolTip : AvaloniaObject
{
    private static readonly DispatcherTimer Timer = new();
    private static Control? _currentTarget;
    private static ToolTipControl? _currentTooltip;

    // Attached property for tooltip content
    public static readonly AttachedProperty<object?> ContentProperty =
        AvaloniaProperty.RegisterAttached<CustomToolTip, Control, object?>(
            "Content",
            defaultValue: null,
            inherits: false);

    // Attached property for tooltip placement
    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<CustomToolTip, Control, PlacementMode>(
            "Placement",
            defaultValue: PlacementMode.Top,
            inherits: false);

    // Attached property for show delay
    public static readonly AttachedProperty<int> ShowDelayProperty =
        AvaloniaProperty.RegisterAttached<CustomToolTip, Control, int>(
            "ShowDelay",
            defaultValue: 500,
            inherits: false);

    // Attached property for offset
    public static readonly AttachedProperty<double> OffsetProperty =
        AvaloniaProperty.RegisterAttached<CustomToolTip, Control, double>(
            "Offset",
            defaultValue: 3.0,
            inherits: false);

    static CustomToolTip()
    {
        Timer.Interval = TimeSpan.FromMilliseconds(500);
        Timer.Tick += OnTimerTick;
        ContentProperty.Changed.AddClassHandler<Control>(OnContentChanged);
    }

    public static void SetContent(Control element, object? value)
    {
        element.SetValue(ContentProperty, value);
    }

    public static object? GetContent(Control element)
    {
        return element.GetValue(ContentProperty);
    }

    public static void SetPlacement(Control element, PlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    public static PlacementMode GetPlacement(Control element)
    {
        return element.GetValue(PlacementProperty);
    }

    public static void SetShowDelay(Control element, int value)
    {
        element.SetValue(ShowDelayProperty, value);
    }

    public static int GetShowDelay(Control element)
    {
        return element.GetValue(ShowDelayProperty);
    }

    public static void SetOffset(Control element, double value)
    {
        element.SetValue(OffsetProperty, value);
    }

    public static double GetOffset(Control element)
    {
        return element.GetValue(OffsetProperty);
    }

    private static void OnContentChanged(Control element, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue != null)
        {
            element.PointerEntered -= OnPointerEntered;
            element.PointerExited -= OnPointerExited;
        }

        if (e.NewValue != null)
        {
            element.PointerEntered += OnPointerEntered;
            element.PointerExited += OnPointerExited;
        }
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Control control) return;

        var content = GetContent(control);
        if (content == null) return;

        System.Diagnostics.Debug.WriteLine($"Tooltip: Pointer entered, content = {content}");

        _currentTarget = control;
        var delay = GetShowDelay(control);
        Timer.Interval = TimeSpan.FromMilliseconds(delay);
        Timer.Start();
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Tooltip: Pointer exited");
        Timer.Stop();
        HideToolTip();
    }

    private static void OnTimerTick(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Tooltip: Timer tick, showing tooltip");
        Timer.Stop();
        ShowToolTip();
    }

    private static void ShowToolTip()
    {
        if (_currentTarget == null)
            return;
        
        var content = GetContent(_currentTarget);
        switch (content)
        {
            case null:
            case "":
                return;
        }

        var placement = GetPlacement(_currentTarget);
        var offset = GetOffset(_currentTarget);

        var window = _currentTarget.GetVisualRoot() as Window;
        if (window == null)
            return;

        var canvas = MainWindow.ToolTipLayerInstance;
        if (canvas == null)
        {
            System.Diagnostics.Debug.WriteLine("Tooltip: No ToolTipLayer canvas found");
            return;
        }

        // Remove previous tooltip, but keep _currentTarget
        if (_currentTooltip != null)
        {
            var parent = _currentTooltip.Parent as Canvas;
            parent?.Children.Remove(_currentTooltip);
            _currentTooltip = null;
        }

        _currentTooltip = new ToolTipControl
        {
            Content = content,
            Target = _currentTarget,
            Placement = placement,
            Offset = offset
        };

        canvas.Children.Add(_currentTooltip);

        // Position AFTER adding to canvas
        PositionToolTip(canvas, _currentTooltip, _currentTarget, placement, offset);

        System.Diagnostics.Debug.WriteLine("Tooltip: Added to canvas");
    }


    private static void PositionToolTip(
        Canvas canvas,
        Control tooltip,
        Control target,
        PlacementMode placement,
        double offset)
    {
        var targetTopLeft = target.TranslatePoint(new Point(0, 0), canvas);
        if (targetTopLeft == null)
            return;

        tooltip.Measure(Size.Infinity);
        var tooltipSize = tooltip.DesiredSize;
        var targetBounds = target.Bounds;

        double x = targetTopLeft.Value.X;
        double y = targetTopLeft.Value.Y;

        switch (placement)
        {
            case PlacementMode.Top:
                x += (targetBounds.Width - tooltipSize.Width) / 2;
                y -= tooltipSize.Height + offset;
                break;

            case PlacementMode.Bottom:
                x += (targetBounds.Width - tooltipSize.Width) / 2;
                y += targetBounds.Height + offset;
                break;

            case PlacementMode.Left:
                x -= tooltipSize.Width + offset;
                y += (targetBounds.Height - tooltipSize.Height) / 2;
                break;

            case PlacementMode.Right:
                x += targetBounds.Width + offset;
                y += (targetBounds.Height - tooltipSize.Height) / 2;
                break;
        }

        Canvas.SetLeft(tooltip, x);
        Canvas.SetTop(tooltip, y);
    }


    private static void HideToolTip()
    {
        if (_currentTooltip != null)
        {
            var parent = _currentTooltip.Parent as Canvas;
            parent?.Children.Remove(_currentTooltip);
            _currentTooltip = null;
        }

        _currentTarget = null;
    }
}