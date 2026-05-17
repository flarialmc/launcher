using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Flarial.Launcher.Controls.AcrylicBlur;

public class AcrylicBlurRenderOperation(
    ImmutableExperimentalAcrylicMaterial material,
    int blur,
    double opacity,
    Rect bounds,
    CornerRadius cornerRadius)
    : ICustomDrawOperation
{
    private static SKShader? _acrylicNoiseShader;

    private readonly ImmutableExperimentalAcrylicMaterial _material = material;
    private readonly double _opacity = ClampOpacity(opacity);
    private readonly Rect _bounds = bounds;
    private readonly CornerRadius _cornerRadius = cornerRadius;
    private SKImage? _backgroundSnapshot;
    private bool _disposed;

    static double ClampOpacity(double value)
    {
        if (value < 0)
            return 0;

        return value > 1 ? 1 : value;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _backgroundSnapshot?.Dispose();
        _disposed = true;
    }

    public bool HitTest(Point p) => _bounds.Contains(p);

    static SKColorFilter CreateAlphaColorFilter(double opacity)
    {
        if (opacity > 1)
            opacity = 1;
        byte[] c = new byte[256];
        byte[] a = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            c[i] = (byte) i;
            a[i] = (byte) (i * opacity);
        }

        return SKColorFilter.CreateTable(a, c, c, c);
    }

    private SKRoundRect CreateRoundRect(float width, float height)
    {
        var rect = SKRect.Create(0, 0, width, height);
        var roundRect = new SKRoundRect();
        
        // Set individual corner radii
        var radii = new[]
        {
            new SKPoint((float)_cornerRadius.TopLeft, (float)_cornerRadius.TopLeft),
            new SKPoint((float)_cornerRadius.TopRight, (float)_cornerRadius.TopRight),
            new SKPoint((float)_cornerRadius.BottomRight, (float)_cornerRadius.BottomRight),
            new SKPoint((float)_cornerRadius.BottomLeft, (float)_cornerRadius.BottomLeft)
        };
        roundRect.SetRectRadii(rect, radii);
        
        
        return roundRect;
    }

    public void Render(ImmediateDrawingContext context)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AcrylicBlurRenderOperation));

        ISkiaSharpApiLeaseFeature? leaseFeature = context.PlatformImpl.GetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
            return;
        using ISkiaSharpApiLease lease = leaseFeature.Lease();

        if (!lease.SkCanvas.TotalMatrix.TryInvert(out SKMatrix currentInvertedTransform) || lease.SkSurface == null)
            return;

        _backgroundSnapshot?.Dispose();
        _backgroundSnapshot = lease.SkSurface.Snapshot();
        
        float width = (float)_bounds.Width;
        float height = (float)_bounds.Height;
        
        // Create rounded rectangle for clipping
        using var roundRect = CreateRoundRect(width, height);
        
        // Save canvas state and apply rounded clip
        lease.SkCanvas.Save();
        lease.SkCanvas.ClipRoundRect(roundRect, SKClipOperation.Intersect, true);
        
        using SKShader? backdropShader = SKShader.CreateImage(_backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);
        using SKSurface? blurred = SKSurface.Create(
            lease.GrContext,
            false,
            new SKImageInfo((int) Math.Ceiling(_bounds.Width), (int) Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul)
        );
        using (SKImageFilter? filter = SKImageFilter.CreateBlur(blur, blur, SKShaderTileMode.Clamp))
        using (SKPaint blurPaint = new SKPaint {Shader = backdropShader, ImageFilter = filter, ColorFilter = CreateAlphaColorFilter(_opacity)})
        {
            blurred.Canvas.DrawRect(0, 0, width, height, blurPaint);

            using (SKImage? blurSnap = blurred.Snapshot())
            using (SKShader? blurSnapShader = SKShader.CreateImage(blurSnap))
            using (SKPaint blurSnapPaint = new SKPaint {Shader = blurSnapShader, IsAntialias = true, ColorFilter = CreateAlphaColorFilter(_opacity)})
            {
                // Rendering twice to reduce opacity
                lease.SkCanvas.DrawRect(0, 0, width, height, blurSnapPaint);
                lease.SkCanvas.DrawRect(0, 0, width, height, blurSnapPaint);
            }

            using SKPaint acrylliPaint = new SKPaint();
            acrylliPaint.IsAntialias = true;
            acrylliPaint.ColorFilter = CreateAlphaColorFilter(_opacity);

            const double noiseOpacity = 0.0225;

            Color tintColor = _material.TintColor;
            SKColor tint = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

            if (_acrylicNoiseShader == null)
            {
                using Stream? stream = typeof(SkiaPlatform).Assembly.GetManifestResourceStream("Avalonia.Skia.Assets.NoiseAsset_256X256_PNG.png");
                using SKBitmap? bitmap = SKBitmap.Decode(stream);
                _acrylicNoiseShader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat).WithColorFilter(CreateAlphaColorFilter(noiseOpacity));
            }

            using (SKShader? backdrop = SKShader.CreateColor(new SKColor(_material.MaterialColor.R, _material.MaterialColor.G, _material.MaterialColor.B, _material.MaterialColor.A)))
            using (SKShader? tintShader = SKShader.CreateColor(tint))
            using (SKShader? effectiveTint = SKShader.CreateCompose(backdrop, tintShader))
            using (SKShader? compose = SKShader.CreateCompose(effectiveTint, _acrylicNoiseShader))
            {
                acrylliPaint.Shader = compose;
                acrylliPaint.IsAntialias = true;
                lease.SkCanvas.DrawRect(0, 0, width, height, acrylliPaint);
            }
        }
        
        // Restore canvas state
        lease.SkCanvas.Restore();
    }

    public Rect Bounds => _bounds.Inflate(4);

    public bool Equals(ICustomDrawOperation? other)
    {
        return other is AcrylicBlurRenderOperation op && 
               op._bounds == _bounds && 
               op._material.Equals(_material) &&
               op._opacity.Equals(_opacity) &&
               op._cornerRadius == _cornerRadius;
    }
}
