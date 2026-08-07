using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Core.Viewport;
using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

public class LocationLayer : MapLayer
{
    public event Action<GeoCoordinate>? OnUpdateCenter;
    private GeoCoordinate? _currentLocation;
    private double? _currentHeading;

    public GeoCoordinate? CurrentLocation
    {
        get => _currentLocation;
        private set
        {
            _currentLocation = value;
            RequestRepaint();
        }
    }

    public double? CurrentHeading
    {
        get => _currentHeading;
        private set
        {
            _currentHeading = value;
            RequestRepaint();
        }
    }

    public LocationLayer(string id = "location-layer") : base(id)
    {
    }

    public void UpdateLocation(GeoCoordinate location, double? heading = null)
    {
        CurrentHeading = heading;
        CurrentLocation = location;
    }

    public override void Dispose()
    {
        // No-Op
    }

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        if (CurrentLocation is null || !IsVisible)
            return;

        if (context.TrackUserLocation)
        {
            var location = CurrentLocation.Value;
            OnUpdateCenter?.Invoke(location);
        }

        var position = WebMercatorProjection.ToCanvasPoint(
            CurrentLocation.Value,
            context.Center,
            context.ZoomLevel,
            context.CanvasSize.Width,
            context.CanvasSize.Height);

        if (position.X < 0 || position.X > info.Width ||
            position.Y < 0 || position.Y > info.Height)
            return;

        if (CurrentHeading.HasValue)
            DrawHeadingCone(canvas, position, CurrentHeading.Value);

        DrawLocationDot(canvas, position);
    }

    private static void DrawLocationDot(SKCanvas canvas, TilePixelPosition position)
    {
        using var outerPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(position.X, position.Y, 12f, outerPaint);

        using var innerPaint = new SKPaint
        {
            Color = new SKColor(0x42, 0x85, 0xF4), // Google blue
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(position.X, position.Y, 9f, innerPaint);
    }

    private static void DrawHeadingCone(SKCanvas canvas, TilePixelPosition position, double heading)
    {
        var headingRad = (float)(heading * Math.PI / 180.0);
        const float coneLength = 40f;
        const float coneHalfAngle = 0.3f;

        var tipX = position.X + (float)(Math.Sin(headingRad) * coneLength);
        var tipY = position.Y - (float)(Math.Cos(headingRad) * coneLength);

        var leftAngle = headingRad - coneHalfAngle;
        var rightAngle = headingRad + coneHalfAngle;

        var leftX = position.X + (float)(Math.Sin(leftAngle) * 15f);
        var leftY = position.Y - (float)(Math.Cos(leftAngle) * 15f);

        var rightX = position.X + (float)(Math.Sin(rightAngle) * 15f);
        var rightY = position.Y - (float)(Math.Cos(rightAngle) * 15f);

        using var pathBuilder = new SKPathBuilder();
        pathBuilder.MoveTo(tipX, tipY);
        pathBuilder.LineTo(leftX, leftY);
        pathBuilder.LineTo(rightX, rightY);
        pathBuilder.Close();

        using var path = pathBuilder.Detach();
        using var conePaint = new SKPaint
        {
            Color = new SKColor(0x42, 0x85, 0xF4, 0xA0),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawPath(path, conePaint);
    }
}