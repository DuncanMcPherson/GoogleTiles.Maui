using GoogleTiles.Maui.Abstractions;
using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

public abstract class MapLayer : IMapLayer
{
    public string Id { get; }
    public bool IsVisible { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;

    public event Action? RepaintRequested;

    protected MapLayer(string id)
    {
        Id = id;
    }

    protected void RequestRepaint() => RepaintRequested?.Invoke();

    void IMapLayer.Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
        => Draw(canvas, info, context);

    protected abstract void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context);
    public abstract void Dispose();
}