using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override SKCanvasView CreatePlatformView()
    {
        _tileFetcher = Services!.GetRequiredService<TileFetcher>();
        _sessionTokenProvider = Services.GetRequiredService<ISessionTokenProvider>();
        return base.CreatePlatformView();
    }

    protected override void ConnectHandler(SKCanvasView platformView)
    {
        platformView.PaintSurface += OnPaintSurface;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(SKCanvasView platformView)
    {
        platformView.PaintSurface -= OnPaintSurface;
        base.DisconnectHandler(platformView);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // TODO: TileCompositor
        // TODO: Attribution overlay
    }

    static partial void MapCenter(GoogleTilesViewHandler handler, GoogleTilesView view)
    {
        handler.PlatformView.SetNeedsDisplay();
    }

    static partial void MapZoomLevel(GoogleTilesViewHandler handler, GoogleTilesView view)
    {
        handler.PlatformView.SetNeedsDisplay();
    }
}