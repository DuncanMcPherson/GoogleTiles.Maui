using System.Collections;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace GoogleTiles.Maui.Layers;

public class PolylineLayer : MapLayer, IEnumerable<Polyline>
{
    private readonly List<Polyline> _polylines = [];

    public PolylineLayer(string id = "polyline-layer") : base(id)
    {
    }

    public void Add(Polyline polyline)
    {
        _polylines.Add(polyline);
        polyline.Positions.CollectionChanged += (sender, args) => RequestRepaint();
        RequestRepaint();
    }

    public void Remove(Polyline polyline)
    {
        _polylines.Remove(polyline);
        RequestRepaint();
    }

    public void Clear()
    {
        _polylines.Clear();
        RequestRepaint();
    }

    public IEnumerator<Polyline> GetEnumerator() => _polylines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override void Dispose() => _polylines.Clear();

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        foreach (var polyline in _polylines.Where(polyline => polyline.Positions.Count >= 2))
        {
            using var path = BuildPath(polyline, context, info);
            if (path is null)
                continue;

            using var paint = ResolvePaint(polyline);
            canvas.DrawPath(path, paint);
        }
    }

    private static SKPath? BuildPath(Polyline polyline, LayerDrawContext context, SKImageInfo info)
    {
        var builder = new SKPathBuilder();
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        for (var i = 0; i < polyline.Positions.Count; i++)
        {
            var position = WebMercatorProjection.ToCanvasPoint(
                polyline.Positions[i],
                context.Center,
                context.ZoomLevel,
                context.RotationDegrees,
                context.CanvasSize.Width,
                context.CanvasSize.Height);
            var point = context.matrix.MapPoint(position.X, position.Y);
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);

            if (i == 0)
                builder.MoveTo(point);
            else
                builder.LineTo(point);
        }

        if (polyline.IsClosed)
            builder.Close();

        const float margin = 32f;
        if (maxX < -margin || minX > info.Width + margin ||
            maxY < -margin || minY > info.Height + margin)
        {
            builder.Dispose();
            return null;
        }

        return builder.Detach();
    }

    private static SKPaint ResolvePaint(Polyline polyline)
    {
        var paint = new SKPaint
        {
            Color = polyline.StrokeColor.ToSKColor(),
            StrokeWidth = polyline.StrokeWidth,
            IsAntialias = true,
            IsStroke = true,
            StrokeCap = polyline.DashPattern is { Length: > 0 } ? SKStrokeCap.Butt : SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        if (polyline.DashPattern is { Length: > 0 })
            paint.PathEffect = SKPathEffect.CreateDash(polyline.DashPattern, 0);
        return paint;
    }
}