using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using GoogleTiles.Maui.Abstractions;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Core.Viewport;
using GoogleTiles.Maui.Models;
using GoogleTiles.Maui.Resolvers;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

public class PinLayer : MapLayer, IEnumerable<Pin>, IRequiresDependencyInjection
{
    private readonly List<Pin> _pins = [];
    private readonly ConcurrentDictionary<Pin, SKBitmap?> _imageCache = new();
    private readonly object _pendingLock = new();
    private readonly HashSet<Pin> _pendingResolutions = new();
    private PinImageResolver? _resolver;
    private SKBitmap? _defaultPin;

    public PinLayer(string id = "pin-layer") : base(id)
    {
    }

    void IRequiresDependencyInjection.InjectDependencies(IServiceProvider services)
    {
        _resolver = services.GetRequiredService<PinImageResolver>();
        _defaultPin = LoadDefaultPin();
    }

    public void Add(Pin pin)
    {
        _pins.Add(pin);
        RequestRepaint();
    }

    public void Remove(Pin pin)
    {
        _pins.Remove(pin);
        _imageCache.TryRemove(pin, out _);
        RequestRepaint();
    }

    public void Clear()
    {
        _pins.Clear();
        _imageCache.Clear();
        RequestRepaint();
    }

    public IEnumerator<Pin> GetEnumerator() => _pins.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override void Dispose()
    {
        _pins.Clear();
        foreach (var bitmap in _imageCache.Values)
            bitmap?.Dispose();
        _imageCache.Clear();
    }

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        foreach (var pin in _pins)
        {
            var position = WebMercatorProjection.ToCanvasPoint(pin.Location,
                context.Center,
                context.ZoomLevel,
                context.CanvasSize.Width,
                context.CanvasSize.Height);

            Debug.WriteLine($"Pin Position: ({position.X}, {position.Y}) | Center: ({context.Center})");

            if (position.X < 0 || position.X > info.Width ||
                position.Y < 0 || position.Y > info.Height)
                continue;

            var bitmap = ResolveBitmap(pin);
            // Image hasn't been loaded yet
            if (bitmap is null)
                continue;
            DrawPin(canvas, bitmap, position, pin);

            if (pin is { ShowLabel: true, Label: not null })
                DrawLabel(canvas, pin.Label, position);
        }
    }

    private void DrawPin(SKCanvas canvas, SKBitmap bitmap, TilePixelPosition position, Pin pin)
    {
        canvas.Save();

        var anchorX = position.X;
        var anchorY = position.Y;

        canvas.Translate(anchorX, anchorY);
        canvas.RotateDegrees(pin.Rotation);
        canvas.Scale(pin.Scale);

        canvas.DrawBitmap(bitmap, new SKPoint(-bitmap.Width / 2f, -bitmap.Height));

        canvas.Restore();
    }

    private void DrawLabel(
        SKCanvas canvas,
        string label,
        TilePixelPosition position)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        using var strokePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 3f
        };

        using var font = new SKFont
        {
            Typeface = SKTypeface.FromFamilyName("sans-serif"),
            Size = 12f,
            Edging = SKFontEdging.Antialias,
        };

        var labelY = position.Y + 4f;

        canvas.DrawText(label, position.X, labelY, SKTextAlign.Center, font, strokePaint);
        canvas.DrawText(label, position.X, labelY, SKTextAlign.Center, font, paint);
    }

    private SKBitmap? ResolveBitmap(Pin pin)
    {
        if (pin.ImageSource is null)
            return _defaultPin;

        if (_imageCache.TryGetValue(pin, out var cached))
            return cached;

        _imageCache[pin] = null;
        QueueResolution(pin);
        return _defaultPin;
    }

    private void QueueResolution(Pin pin)
    {
        lock (_pendingLock)
        {
            if (!_pendingResolutions.Add(pin))
                return;
        }

        Task.Run(async () =>
        {
            try
            {
                var bitmap = await _resolver!.ResolveAsync(pin.ImageSource);
                _imageCache[pin] = bitmap;
            }
            catch (Exception ex)
            {
                _imageCache.TryRemove(pin, out _);
                Debug.WriteLine($"Pin image resolution failed: {ex.Message}");
            }
            finally
            {
                lock (_pendingLock)
                    _pendingResolutions.Remove(pin);
                MainThread.BeginInvokeOnMainThread(RequestRepaint);
            }
        });
    }

    private static SKBitmap? LoadDefaultPin()
    {
        var assembly = typeof(PinLayer).Assembly;
        using var stream = assembly.GetManifestResourceStream("GoogleTiles.Maui.Resources.default_pin.png");
        return stream is null ? null : SKBitmap.Decode(stream);
    }
}