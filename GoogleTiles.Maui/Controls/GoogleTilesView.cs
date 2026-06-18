using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace GoogleTiles.Maui.Controls;

public class GoogleTilesView : SKCanvasView
{
    #region Bindable Properties

    public static readonly BindableProperty CenterProperty = BindableProperty.Create(
        nameof(Center),
        typeof(GeoCoordinate),
        typeof(GoogleTilesView),
        new GeoCoordinate(0, 0));

    public static readonly BindableProperty ZoomLevelProperty = BindableProperty.Create(
        nameof(ZoomLevel),
        typeof(int),
        typeof(GoogleTilesView),
        15);

    public GeoCoordinate Center
    {
        get => (GeoCoordinate)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public int ZoomLevel
    {
        get => (int)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    #endregion

    private SKSizeI _canvasSize;
    private TileFetcher _tileFetcher;
    private ISessionTokenProvider _sessionTokenProvider;

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);
        _canvasSize = e.Info.Size;
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // TODO: Tile compositor
        // TODO: attribution overlay
    }

    internal void Initialize(TileFetcher tileFetcher, ISessionTokenProvider sessionTokenProvider)
    {
        _tileFetcher = tileFetcher;
        _sessionTokenProvider = sessionTokenProvider;
    }

    internal void Cleanup()
    {
    }
}