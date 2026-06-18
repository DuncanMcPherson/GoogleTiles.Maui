using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp.Views.Android;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override void ConnectHandler(SKCanvasView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Initialize(Services!.GetRequiredService<TileFetcher>(), Services!.GetRequiredService<ISessionTokenProvider>());
        }
    }

    protected override void DisconnectHandler(SKCanvasView platformView)
    {
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Cleanup();
        }
        base.DisconnectHandler(platformView);
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