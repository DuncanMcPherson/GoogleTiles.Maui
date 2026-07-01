using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Abstractions;

internal interface IMapLayer : IDisposable
{
    string Id { get; }
    bool IsVisible { get; set; }
    float Opacity { get; set; }
    void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context);
}