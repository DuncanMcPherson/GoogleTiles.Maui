using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp;
using SkiaSharp.Views.Android;
using SKCanvasView = SkiaSharp.Views.Android.SKCanvasView;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override SKCanvasView CreatePlatformView()
    {
        _tileFetcher = Services!.GetRequiredService<TileFetcher>();
        _sessionTokenProvider = Services!.GetRequiredService<ISessionTokenProvider>();

        var platformView = new SKCanvasView(Context);
        return platformView;
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
        handler.PlatformView.Invalidate();
    }

    static partial void MapZoomLevel(GoogleTilesViewHandler handler, GoogleTilesView view)
    {
        handler.PlatformView.Invalidate();
    }
}