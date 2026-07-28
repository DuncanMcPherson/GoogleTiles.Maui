using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Gestures;
using SkiaSharp.Views.iOS;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    protected override void ConnectHandler(SKMetalView platformView)
    {
        base.ConnectHandler(platformView);
        if (_rotationGestureHandler is null)
        {
            _rotationGestureHandler = new RotationGestureHandler();
            _rotationGestureHandler.RotationDeltaChanged += OnRotationDeltaChanged;
            _rotationGestureHandler.Attach(platformView);
        }
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
        if (_rotationGestureHandler is not null)
        {
            _rotationGestureHandler.Detach(platformView);
            _rotationGestureHandler.RotationDeltaChanged -= OnRotationDeltaChanged;
            _rotationGestureHandler = null;
        }
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Cleanup();
        }
        base.DisconnectHandler(platformView);
    }

    private void OnRotationDeltaChanged(float delta)
    {
        if (VirtualView is GoogleTilesView view)
            MainThread.BeginInvokeOnMainThread(() => view.MapRotation += delta);
    }
}