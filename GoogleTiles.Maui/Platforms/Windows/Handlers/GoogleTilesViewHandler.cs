using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp.Views.Windows;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override void ConnectHandler(SKXamlCanvas platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Initialize(
                Services!.GetRequiredService<TileFetcher>(),
                Services!.GetRequiredService<ISessionTokenProvider>(),
                Services!.GetRequiredService<GoogleTilesOptions>(),
                Services!.GetRequiredService<ViewportMetadataFetcher>());
        }
    }

    protected override void DisconnectHandler(SKXamlCanvas platformView)
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