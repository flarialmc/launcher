using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;

namespace Flarial.Launcher.Controls.ToolTip;

public class ToolTipControl : ContentControl
{
    const int AnimationDurationMs = 220;

    private Control? _geometryHost;
    private AcrylicBlur.AcrylicBlur? _acrylic;
    private Path? _border;
    private double _arrowCenterX;

    
    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<ToolTipControl, Control?>(nameof(Target));

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<ToolTipControl, PlacementMode>(
            nameof(Placement), 
            defaultValue: PlacementMode.Top);

    public static readonly StyledProperty<double> OffsetProperty =
        AvaloniaProperty.Register<ToolTipControl, double>(
            nameof(Offset), 
            defaultValue: 8.0);

    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    static ToolTipControl()
    {
        AffectsArrange<ToolTipControl>(TargetProperty, PlacementProperty, OffsetProperty, BoundsProperty);
    }
    
    public ToolTipControl()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        IsHitTestVisible = false; // Don't intercept pointer events
    }

    private void UpdateGeometry()
    {
        if (_geometryHost == null || _acrylic == null || _border == null)
            return;

        var w = _geometryHost.Bounds.Width;
        var h = _geometryHost.Bounds.Height;

        if (w <= 0 || h <= 0)
            return;

        const double r = 10;          // Corner Radius
        const double arrowW = 12;     // Arrow Width at base
        const double arrowH = 16;     // Arrow Height
        
        // Ensure the arrow stays within the rounded corners
        var center = ClampArrowCenter(_arrowCenterX, w);
        
        var g = new StreamGeometry();

        using (var ctx = g.Open())
        {
            // === PLACEMENT: BOTTOM (Arrow points UP) ===
            if (Placement == PlacementMode.Bottom)
            {
                // The "body" starts at Y = arrowH and goes to h

                // Start at Top-Left of the body (after the corner)
                ctx.BeginFigure(new Point(r, arrowH), true);

                // 1. Top Edge (Left side) -> Arrow -> Top Edge (Right side)
                ctx.LineTo(new Point(center - arrowW / 2, arrowH));
                ctx.LineTo(new Point(center, 0)); // The Tip (pointing up)
                ctx.LineTo(new Point(center + arrowW / 2, arrowH));
                ctx.LineTo(new Point(w - r, arrowH));

                // 2. Top-Right Corner
                ctx.ArcTo(new Point(w, arrowH + r), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 3. Right Edge
                ctx.LineTo(new Point(w, h - r));

                // 4. Bottom-Right Corner
                ctx.ArcTo(new Point(w - r, h), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 5. Bottom Edge
                ctx.LineTo(new Point(r, h));

                // 6. Bottom-Left Corner
                ctx.ArcTo(new Point(0, h - r), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 7. Left Edge
                ctx.LineTo(new Point(0, arrowH + r));

                // 8. Top-Left Corner (closing the loop)
                ctx.ArcTo(new Point(r, arrowH), new Size(r, r), 0, false, SweepDirection.Clockwise);
            }
            // === PLACEMENT: TOP / DEFAULT (Arrow points DOWN) ===
            else 
            {
                // The "body" starts at Y = 0 and goes to h - arrowH
                var bodyBottom = h - arrowH;

                ctx.BeginFigure(new Point(r, 0), true);

                // 1. Top Edge
                ctx.LineTo(new Point(w - r, 0));
                ctx.ArcTo(new Point(w, r), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 2. Right Edge
                ctx.LineTo(new Point(w, bodyBottom - r));
                ctx.ArcTo(new Point(w - r, bodyBottom), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 3. Bottom Edge (Right side) -> Arrow -> Bottom Edge (Left side)
                ctx.LineTo(new Point(center + arrowW / 2, bodyBottom));
                ctx.LineTo(new Point(center, h)); // The Tip (pointing down)
                ctx.LineTo(new Point(center - arrowW / 2, bodyBottom));
                ctx.LineTo(new Point(r, bodyBottom));

                // 4. Bottom-Left Corner
                ctx.ArcTo(new Point(0, bodyBottom - r), new Size(r, r), 0, false, SweepDirection.Clockwise);

                // 5. Left Edge
                ctx.LineTo(new Point(0, r));
                ctx.ArcTo(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise);
            }
        }

        _acrylic.Clip = g;
        _border.Data = g;
    }
    
    private void UpdatePadding()
    {
        const double arrowH = 16;     // Must match UpdateGeometry
        const double contentMargin = 10; // General padding inside the border

        // If arrow is on Top (Placement=Bottom), push content down.
        // If arrow is on Bottom (Placement=Top), push content up (bottom padding).
        Padding = Placement == PlacementMode.Bottom ? new Thickness(contentMargin, contentMargin + arrowH, contentMargin, contentMargin) 
            : new Thickness(contentMargin, contentMargin, contentMargin, contentMargin + arrowH);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InvalidateMeasure();
        Dispatcher.UIThread.Post(() => Classes.Add("open"), DispatcherPriority.Render);
    }

    public async Task HideAsync()
    {
        Classes.Remove("open");
        await Task.Delay(AnimationDurationMs);
    }

    public void HideImmediately()
    {
        Classes.Remove("open");
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        _geometryHost = e.NameScope.Find<Control>("PART_GeometryHost");
        _acrylic = e.NameScope.Find<AcrylicBlur.AcrylicBlur>("PART_Acrylic");
        _border = e.NameScope.Find<Path>("PART_Border");

        _geometryHost?.GetObservable(BoundsProperty)
            .Subscribe(_ => UpdateGeometry());
        
        // Initial setup
        UpdatePadding();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PlacementProperty)
        {
            UpdatePadding();
            UpdateGeometry(); // Force redraw of arrow direction
            InvalidateArrange();
        }
        else if (change.Property == TargetProperty || change.Property == OffsetProperty)
        {
            InvalidateArrange();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Allow the tooltip to measure without constraints
        return base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        
        // Only update position if we have a target
        if (Target != null)
            UpdatePosition(result);
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (Target != null) InvalidateArrange();
            }, DispatcherPriority.Render);
        }
        
        return result;
    }

    private void UpdatePosition(Size tooltipSize)
    {
        if (Target is not { IsVisible: true }) return;

        if (Parent is not Control container) return;

        // Get target position relative to the container
        var targetPosition = Target.TranslatePoint(new Point(0, 0), container);
        if (!targetPosition.HasValue) return;

        var targetBounds = new Rect(targetPosition.Value, Target.Bounds.Size);
        var toolTipWidth = tooltipSize.Width;
        var toolTipHeight = tooltipSize.Height;

        Point position = CalculatePosition(targetBounds, toolTipWidth, toolTipHeight);

        // Ensure tooltip stays within container bounds
        var containerBounds = container.Bounds;
        position = ClampToBounds(position, toolTipWidth, toolTipHeight, containerBounds);

        // Calculate arrow center relative to tooltip
        var targetCenterX = targetBounds.X + targetBounds.Width / 2;

        // Convert to tooltip-local coordinates
        // Geometry host position relative to container
        var hostPos = _geometryHost?.TranslatePoint(new Point(0, 0), container);

        if (hostPos.HasValue)
        {
            _arrowCenterX = targetCenterX - hostPos.Value.X;
        }



        // Use Canvas positioning if the container is a Canvas, otherwise use RenderTransform
        if (container is Canvas)
        {
            Canvas.SetLeft(this, position.X);
            Canvas.SetTop(this, position.Y);
        }
        else RenderTransform = new TranslateTransform(position.X, position.Y);
        
        UpdateGeometry();

    }

    private double ClampArrowCenter(double x, double width)
    {
        const double cornerRadius = 10;
        const double arrowHalfWidth = 6; // arrowW / 2
        const double margin = cornerRadius + arrowHalfWidth + 2;

        return Math.Min(Math.Max(x, margin), width - margin);
    }

    
    private Point CalculatePosition(Rect targetBounds, double tooltipWidth, double tooltipHeight)
    {
        return Placement switch
        {
            PlacementMode.Top => new Point(
                targetBounds.X + (targetBounds.Width - tooltipWidth) / 2,
                targetBounds.Y - tooltipHeight - Offset),
            
            PlacementMode.Bottom => new Point(
                targetBounds.X + (targetBounds.Width - tooltipWidth) / 2,
                targetBounds.Y + targetBounds.Height + Offset),
            
            PlacementMode.Left => new Point(
                targetBounds.X - tooltipWidth - Offset,
                targetBounds.Y + (targetBounds.Height - tooltipHeight) / 2),
            
            PlacementMode.Right => new Point(
                targetBounds.X + targetBounds.Width + Offset,
                targetBounds.Y + (targetBounds.Height - tooltipHeight) / 2),
            
            _ => new Point(
                targetBounds.X + (targetBounds.Width - tooltipWidth) / 2,
                targetBounds.Y - tooltipHeight - Offset)
        };
    }

    private Point ClampToBounds(Point position, double width, double height, Rect bounds)
    {
        const double margin = 8;
        return new Point(
            Math.Max(margin, Math.Min(position.X, bounds.Width - width - margin)),
            Math.Max(margin, Math.Min(position.Y, bounds.Height - height - margin))
        );
    }
}
