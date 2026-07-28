using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp.Views.iOS;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override void ConnectHandler(SKMetalView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Initialize(
                Services!.GetRequiredService<TileFetcher>(),
                Services!.GetRequiredService<ISessionTokenProvider>(),
                Services!.GetRequiredService<SessionTokenCache>(),
                Services!.GetRequiredService<GoogleTilesOptions>(),
                Services!.GetRequiredService<ViewportMetadataFetcher>(),
                _rotationGestureHandler);
        }
    }

    protected override void DisconnectHandler(SKMetalView platformView)
    {
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Cleanup();
        }
        base.DisconnectHandler(platformView);
    }
}