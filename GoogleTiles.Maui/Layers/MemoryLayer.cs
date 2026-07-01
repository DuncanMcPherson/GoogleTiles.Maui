using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

public class MemoryLayer : MapLayer
{
    public Action<SKCanvas, LayerDrawContext>? DrawAction { get; set; }

    public MemoryLayer(string id) : base(id)
    {
    }

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        DrawAction?.Invoke(canvas, context);
        RequestRepaint();
    }

    public override void Dispose()
    {
        // No op for now
    }
}