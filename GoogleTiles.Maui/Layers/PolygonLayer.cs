using System.Collections;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace GoogleTiles.Maui.Layers;

public class PolygonLayer : MapLayer, IEnumerable<Polygon>
{
    private readonly List<Polygon> _polygons = [];

    public PolygonLayer(string id = "polygon-layer") : base(id) {}

    public void Add(Polygon polygon)
    {
        _polygons.Add(polygon);
        RequestRepaint();
    }

    public void Remove(Polygon polygon)
    {
        _polygons.Remove(polygon);
        RequestRepaint();
    }

    public void Clear()
    {
        _polygons.Clear();
        RequestRepaint();
    }

    public IEnumerator<Polygon> GetEnumerator() => _polygons.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override void Dispose() => _polygons.Clear();

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        foreach (var p in _polygons)
        {
            using var path = BuildPath(p, context, info);
            if (path is null)
                continue;

            using var fillPaint = ResolvePaint(p.FillColor, true);
            if (fillPaint is not null)
            {
                canvas.DrawPath(path, fillPaint);
            }

            using var edgePaint = ResolvePaint(p.EdgeColor, false, p.StrokeWidth);
            if (edgePaint is null) continue;
            canvas.DrawPath(path, edgePaint);
        }
    }

    private static SKPath? BuildPath(Polygon polygon, LayerDrawContext context, SKImageInfo info)
    {
        if (polygon.Positions.Count < 3)
            return null;
        using var builder = new SKPathBuilder();
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        for (var i = 0; i < polygon.Positions.Count; i++)
        {
            var position = WebMercatorProjection.ToCanvasPoint(
                polygon.Positions[i],
                context.Center,
                context.ZoomLevel,
                context.CanvasSize.Width,
                context.CanvasSize.Height);
            var point = context.Matrix.MapPoint(position.X, position.Y);
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);

            if (i == 0)
                builder.MoveTo(point);
            else
                builder.LineTo(point);
        }

        // All polygons need to be closed
        builder.Close();

        const float margin = 32f;
        if (maxX < -margin || minX > info.Width + margin ||
            maxY < -margin || minY > info.Height + margin)
        {
            return null;
        }

        return builder.Detach();
    }

    private static SKPaint? ResolvePaint(Color? color, bool isFill, float strokeWidth = 4f)
    {
        if (color is null)
            return null;

        var paint = new SKPaint();
        paint.Color = color.ToSKColor();
        paint.IsAntialias = true;
        paint.Style = isFill ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
        if (!isFill)
        {
            paint.StrokeWidth = strokeWidth;
            paint.StrokeJoin = SKStrokeJoin.Round;
            paint.StrokeCap = SKStrokeCap.Round;
        }

        return paint;
    }
}