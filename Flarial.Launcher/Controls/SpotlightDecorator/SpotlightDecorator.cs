using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Flarial.Launcher.Controls.SpotlightDecorator;

public class SpotlightDecorator : Decorator
{
    private SpotlightAdorner? _adorner;

    // 1. New Property for Animation
    // We will animate this value from 0.0 to 1.0
    public static readonly StyledProperty<double> SpotlightOpacityProperty =
        AvaloniaProperty.Register<SpotlightDecorator, double>(nameof(SpotlightOpacity), 0.0);

    public double SpotlightOpacity
    {
        get => GetValue(SpotlightOpacityProperty);
        set => SetValue(SpotlightOpacityProperty, value);
    }

    public static readonly StyledProperty<Color> SpotlightColorProperty =
        AvaloniaProperty.Register<SpotlightDecorator, Color>(nameof(SpotlightColor), Colors.Cyan);

    public Color SpotlightColor
    {
        get => GetValue(SpotlightColorProperty);
        set => SetValue(SpotlightColorProperty, value);
    }

    public static readonly StyledProperty<double> SpotlightRadiusProperty =
        AvaloniaProperty.Register<SpotlightDecorator, double>(nameof(SpotlightRadius), 150.0);

    public double SpotlightRadius
    {
        get => GetValue(SpotlightRadiusProperty);
        set => SetValue(SpotlightRadiusProperty, value);
    }

    public SpotlightDecorator()
    {
        // 2. Configure Smooth Transitions
        // This tells Avalonia: "Whenever SpotlightOpacity is changed, take 200ms to transition to the new value."
        Transitions = new Avalonia.Animation.Transitions
        {
            new Avalonia.Animation.DoubleTransition
            {
                Property = SpotlightOpacityProperty,
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new Avalonia.Animation.Easings.SineEaseOut()
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_adorner != null) return;
        _adorner = new SpotlightAdorner(this);
        AdornerLayer.GetAdornerLayer(this)?.Children.Add(_adorner);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_adorner != null)
        {
            AdornerLayer.GetAdornerLayer(this)?.Children.Remove(_adorner);
            _adorner = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // Important: Re-render when Opacity changes during the animation
        if (change.Property == SpotlightColorProperty || 
            change.Property == SpotlightRadiusProperty || 
            change.Property == SpotlightOpacityProperty)
        {
            _adorner?.InvalidateVisual();
        }
    }

    private class SpotlightAdorner : Control
    {
        private readonly SpotlightDecorator _host;
        private SKRuntimeEffect? _effect;
        private Point _mousePosition;
        private RenderTargetBitmap? _buffer;

        public SpotlightAdorner(SpotlightDecorator host)
        {
            _host = host;
            IsHitTestVisible = false;
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            // Updated Shader: Added 'uOpacity'
            const string shaderCode = @"
                uniform shader content;
                uniform float2 uMouse;
                uniform float3 uColor;
                uniform float uRadius;
                uniform float uDpi;
                uniform float uOpacity; // Controls fade in/out

                half4 main(float2 coord) {
                    float2 p = coord * uDpi;
                    half4 c = content.eval(p);
                    
                    if (c.a < 0.01) return c;

                    // Edge Detection
                    float aTop    = content.eval(p + float2(0, -1)).a;
                    float aBottom = content.eval(p + float2(0,  1)).a;
                    float aLeft   = content.eval(p + float2(-1, 0)).a;
                    float aRight  = content.eval(p + float2( 1, 0)).a;

                    float delta = abs(c.a - aTop) + 
                                  abs(c.a - aBottom) + 
                                  abs(c.a - aLeft) + 
                                  abs(c.a - aRight);

                    // Thinner Borders Fix:
                    // Using smoothstep(0.1, 0.5, ...) ignores subtle anti-aliasing pixels
                    // and only lights up the sharp edge.
                    float borderIntensity = smoothstep(0.1, 0.5, delta);

                    // Spotlight
                    float dist = distance(coord, uMouse);
                    float spotIntensity = smoothstep(uRadius, 0.0, dist);

                    // Mix Intensities
                    half3 light = uColor;
                    float finalIntensity = mix(0.15, 0.8, borderIntensity * 0.8);

                    float overlayAlpha = spotIntensity * finalIntensity * uOpacity * 0.45;
                    return half4(light * overlayAlpha, overlayAlpha);
                }";

            _effect = SKRuntimeEffect.CreateShader(shaderCode, out string error);
            if (_effect == null) throw new Exception(error);

            // 3. Logic: Trigger Fades on Input
            _host.PointerMoved += (s, e) => {
                _mousePosition = e.GetPosition(_host);
                // Trigger Fade In
                _host.SpotlightOpacity = 1.0;
                InvalidateVisual();
            };
            
            _host.PointerEntered += (s, e) => {
                 // Trigger Fade In
                _host.SpotlightOpacity = 1.0;
            };

            _host.PointerExited += (s, e) => {
                // Trigger Fade Out
                // Note: We DO NOT reset _mousePosition here. 
                // This lets the light fade out right where the mouse left.
                _host.SpotlightOpacity = 0.0;
            };
        }
        
        public override void Render(DrawingContext context)
        {
            if (_host.Child == null || _host.Bounds.Width <= 0 || _host.Bounds.Height <= 0) return;

            // Optimization: If invisible, don't render anything
            if (_host.SpotlightOpacity <= 0.01) return;

            var topLevel = TopLevel.GetTopLevel(_host);
            var dpi = topLevel?.RenderScaling ?? 1.0;

            var transform = _host.TransformToVisual(this);
            if (transform == null) return;
            var offset = transform.Value.Transform(new Point(0, 0));

            var pixelWidth = (int)(_host.Bounds.Width * dpi);
            var pixelHeight = (int)(_host.Bounds.Height * dpi);
            var pixelSize = new PixelSize(pixelWidth, pixelHeight);

            if (_buffer == null || _buffer.PixelSize != pixelSize)
            {
                _buffer?.Dispose();
                _buffer = new RenderTargetBitmap(pixelSize, new Vector(96 * dpi, 96 * dpi));
            }

            _buffer.Render(_host);

            context.Custom(new ShaderDrawOperation(
                new Rect(offset.X, offset.Y, _host.Bounds.Width, _host.Bounds.Height),
                _effect!, 
                _mousePosition, 
                (float)dpi,
                _host.SpotlightColor,
                (float)_host.SpotlightRadius,
                (float)_host.SpotlightOpacity,
                _buffer
            ));
        }
    }

    private class ShaderDrawOperation : ICustomDrawOperation
    {
        public Rect Bounds { get; }
        private readonly SKRuntimeEffect _effect;
        private readonly Point _mouse;
        private readonly float _dpi;
        private readonly Color _color;
        private readonly float _radius;
        private readonly float _opacity;
        private readonly RenderTargetBitmap _bmp;

        public ShaderDrawOperation(Rect bounds, SKRuntimeEffect effect, Point mouse, float dpi, Color color, float radius, float opacity, RenderTargetBitmap bmp)
        {
            Bounds = bounds;
            _effect = effect;
            _mouse = mouse;
            _dpi = dpi;
            _color = color;
            _radius = radius;
            _opacity = opacity;
            _bmp = bmp;
        }

        public void Dispose() { }
        public bool Equals(ICustomDrawOperation? other) => false;
        public bool HitTest(Point p) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var lease = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (lease == null) return;
            using var skia = lease.Lease();
            var canvas = skia.SkCanvas;

            canvas.Save();
            canvas.Translate((float)Bounds.X, (float)Bounds.Y);

            // Using Rgba8888 based on your feedback that colors were inverted
            var info = new SKImageInfo(_bmp.PixelSize.Width, _bmp.PixelSize.Height, SKColorType.Rgba8888);
            
            using var pixelMap = new SKPixmap(info, Marshal.AllocHGlobal(info.BytesSize));
            
            try
            {
                _bmp.CopyPixels(new PixelRect(_bmp.PixelSize), pixelMap.GetPixels(), info.BytesSize, info.RowBytes);
                
                using var contentImage = SKImage.FromPixels(pixelMap);
                using var contentShader = contentImage.ToShader(SKShaderTileMode.Decal, SKShaderTileMode.Decal);

                var uniforms = new SKRuntimeEffectUniforms(_effect);
                uniforms["uMouse"] = new[] { (float)_mouse.X, (float)_mouse.Y };
                uniforms["uDpi"] = _dpi;
                uniforms["uColor"] = new[] { (float)_color.R / 255f, (float)_color.G / 255f, (float)_color.B / 255f };
                uniforms["uRadius"] = _radius;
                uniforms["uOpacity"] = _opacity; // Pass current animation state

                var children = new SKRuntimeEffectChildren(_effect);
                children["content"] = contentShader;

                using var shader = _effect.ToShader(uniforms, children);
                using var paint = new SKPaint { Shader = shader };

                canvas.DrawRect(SKRect.Create((float)Bounds.Width, (float)Bounds.Height), paint);
            }
            finally
            {
                Marshal.FreeHGlobal(pixelMap.GetPixels());
                canvas.Restore();
            }
        }
    }
}
