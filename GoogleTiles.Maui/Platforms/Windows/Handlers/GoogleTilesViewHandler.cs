using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Gestures;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler
{
    private PanGestureHandler? _panGestureHandler;
    private ScaleGestureHandler? _scaleGestureHandler;
    protected override void ConnectHandler(SKSwapChainPanel platformView)
    {
        base.ConnectHandler(platformView);
        if (_rotationGestureHandler is null)
        {
            _rotationGestureHandler = new RotationGestureHandler();
            _rotationGestureHandler.RotationDeltaChanged += OnRotationDeltaChanged;
            _rotationGestureHandler.Attach(platformView);
        }

        if (_panGestureHandler is null)
        {
            _panGestureHandler = new PanGestureHandler();
            _panGestureHandler.PanDeltaChanged += OnPanDeltaChanged;
            _panGestureHandler.Attach(platformView);
        }

        if (_scaleGestureHandler is null)
        {
            _scaleGestureHandler = new();
            _scaleGestureHandler.ZoomDeltaChanged += OnZoomDeltaChanged;
            _scaleGestureHandler.Attach(platformView);
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

        platformView.PointerWheelChanged += OnPointerWheelChanged;
    }

    protected override void DisconnectHandler(SKSwapChainPanel platformView)
    {
        if (_rotationGestureHandler is not null)
        {
            _rotationGestureHandler.Detach(platformView);
            _rotationGestureHandler.RotationDeltaChanged -= OnRotationDeltaChanged;
            _rotationGestureHandler = null;
        }

        if (_panGestureHandler is not null)
        {
            _panGestureHandler.Detach(platformView);
            _panGestureHandler.PanDeltaChanged -= OnPanDeltaChanged;
            _panGestureHandler = null;
        }

        if (_scaleGestureHandler is not null)
        {
            _scaleGestureHandler.Detach(platformView);
            _scaleGestureHandler.ZoomDeltaChanged -= OnZoomDeltaChanged;
            _scaleGestureHandler = null;
        }
        if (VirtualView is GoogleTilesView gtView)
        {
            gtView.Cleanup();
        }

        platformView.PointerWheelChanged -= OnPointerWheelChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnPointerWheelChanged(object? sender, PointerRoutedEventArgs e)
    {
        if (VirtualView is not GoogleTilesView view) return;
        var rawDelta = e.GetCurrentPoint((UIElement)sender!).Properties.MouseWheelDelta;
        var notches = rawDelta / 120f;
        var scaleDelta = (float)Math.Pow(2, notches * 0.25f);
        view.OnScrollZoom(scaleDelta);
        e.Handled = true;
    }

    private void OnRotationDeltaChanged(float delta)
    {
        if (VirtualView is GoogleTilesView view)
            MainThread.BeginInvokeOnMainThread(() => view.MapRotation += delta);
    }

    private void OnPanDeltaChanged(float x, float y)
    {
        if (VirtualView is GoogleTilesView view)
            MainThread.BeginInvokeOnMainThread(() => view.OnPan(x, y));
    }

    private void OnZoomDeltaChanged(float delta)
    {
        if (VirtualView is GoogleTilesView view)
            view.OnScrollZoom(delta);
    }
}