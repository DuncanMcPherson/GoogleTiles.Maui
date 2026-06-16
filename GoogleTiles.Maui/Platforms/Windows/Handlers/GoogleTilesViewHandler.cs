using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override SKXamlCanvas CreatePlatformView()
    {
        _tileFetcher = Services!.GetRequiredService<TileFetcher>();
        _sessionTokenProvider = Services!.GetRequiredService<ISessionTokenProvider>();
        return base.CreatePlatformView();
    }

    protected override void ConnectHandler(SKXamlCanvas platformView)
    {
        platformView.PaintSurface += OnPaintSurface;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(SKXamlCanvas platformView)
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