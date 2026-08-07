using System.Collections;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace GoogleTiles.Maui.Layers;

public class CirclesLayer : MapLayer, IEnumerable<Circle>
{
    private readonly List<Circle> _circles = [];

    public CirclesLayer(string id = "circles-layer") : base(id)
    {
    }

    public void Add(Circle circle)
    {
        _circles.Add(circle);
        RequestRepaint();
    }

    public void Remove(Circle circle)
    {
        _circles.Remove(circle);
        RequestRepaint();
    }

    public void Clear()
    {
        _circles.Clear();
        RequestRepaint();
    }

    public IEnumerator<Circle> GetEnumerator() => _circles.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override void Dispose() => _circles.Clear();

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        foreach (var circle in _circles)
        {
            var center = WebMercatorProjection.ToCanvasPoint(circle.Position, context.Center, context.ZoomLevel,
                context.CanvasSize.Width, context.CanvasSize.Height);
            var radiusPx =
                WebMercatorProjection.GetRadiusInPixels(circle.RadiusInFeet, circle.Position.Latitude,
                    context.ZoomLevel);
            var point = context.Matrix.MapPoint(center.X, center.Y);

            if (circle.FillColor is { } fill)
            {
                using var fillPaint = new SKPaint();
                fillPaint.Color = fill.ToSKColor();
                fillPaint.Style = SKPaintStyle.Fill;
                fillPaint.IsAntialias = true;
                canvas.DrawCircle(point, radiusPx, fillPaint);
            }

            if (circle.EdgeColor is not { } edge) continue;
            using var edgePaint = new SKPaint();
            edgePaint.Color = edge.ToSKColor();
            edgePaint.Style = SKPaintStyle.Stroke;
            edgePaint.StrokeWidth = circle.StrokeWidth;
            edgePaint.IsAntialias = true;
            canvas.DrawCircle(point, radiusPx, edgePaint);
        }
    }
}