using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
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
                Services!.GetRequiredService<SessionTokenCache>(),
                Services!.GetRequiredService<GoogleTilesOptions>(),
                Services!.GetRequiredService<ViewportMetadataFetcher>());
        }

        platformView.PointerWheelChanged += OnPointerWheelChanged;
    }

    protected override void DisconnectHandler(SKXamlCanvas platformView)
    {
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
}