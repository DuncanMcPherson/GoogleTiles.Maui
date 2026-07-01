using System.Collections.Concurrent;
using System.Diagnostics;
using GoogleTiles.Maui.Abstractions;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Layers;
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

    public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(
        nameof(MapType),
        typeof(MapType),
        typeof(GoogleTilesView),
        MapType.Roadmap,
        propertyChanged: OnMapTypeChanged);

    public static readonly BindableProperty MapThemeProperty = BindableProperty.Create(
        nameof(MapTheme),
        typeof(MapTheme),
        typeof(GoogleTilesView),
        MapTheme.Day,
        propertyChanged: OnMapThemeChanged);

    public GeoCoordinate Center
    {
        get => (GeoCoordinate)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public int ZoomLevel
    {
        get => (int)GetValue(ZoomLevelProperty);
        set
        {
            SetValue(ZoomLevelProperty, value);
            _baseZoomLevel = value;
        }
    }

    public MapType MapType
    {
        get => (MapType)GetValue(MapTypeProperty);
        set => SetValue(MapTypeProperty, value);
    }

    public MapTheme MapTheme
    {
        get => (MapTheme)GetValue(MapThemeProperty);
        set => SetValue(MapThemeProperty, value);
    }

    #endregion

    #region State

    private SKSizeI _canvasSize;
    private TileFetcher _tileFetcher;
    private GoogleTilesOptions _options;
    private PointF _lastPanPosition;
    private double _accumulatedScale = 1.0;
    private double _zoomScale = 1.0;
    private int _baseZoomLevel;

    private readonly List<IMapLayer> _layers = [];
    private TileLayer _tileLayer;
    private AttributionLayer _attributionLayer;

    private CancellationTokenSource _cts = new();

    #endregion

    #region Screen Updates

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        if (_tileFetcher is null || _options is null)
            return;
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        _canvasSize = e.Info.Size;

        var context = new LayerDrawContext(
            Center,
            ZoomLevel,
            _zoomScale,
            _canvasSize);
        if (_zoomScale != 1.0)
        {
            canvas.Save();
            canvas.Scale((float)_zoomScale, (float)_zoomScale);
        }

        foreach (var layer in _layers.Where(l => l.IsVisible))
        {
            if (_zoomScale != 1.0 && layer is AttributionLayer)
                canvas.Restore();
            if (layer.Opacity < 1.0f)
            {
                canvas.SaveLayer(new SKPaint { Color = SKColors.White.WithAlpha((byte)(layer.Opacity * 255)) });
                layer.Draw(canvas, e.Info, context);
                canvas.Restore();
            }
            else
            {
                layer.Draw(canvas, e.Info, context);
            }
        }
    }

    #endregion

    #region Setup

    internal void Initialize(TileFetcher tileFetcher, ISessionTokenProvider sessionTokenProvider,
        SessionTokenCache cache,
        GoogleTilesOptions options, ViewportMetadataFetcher metadataFetcher)
    {
        _tileFetcher = tileFetcher;
        _options = options;
        var logoBytes = LoadEmbeddedResource("GoogleTiles.Maui.Resources.googlemaps_logo_withdarkoutline_1x.png");
        _tileLayer = new TileLayer("tile-layer", tileFetcher, cache, options);
        _tileLayer.RepaintRequested += InvalidateSurface;

        _attributionLayer = new AttributionLayer("attribution-layer", metadataFetcher, logoBytes);
        _attributionLayer.RepaintRequested += InvalidateSurface;

        _layers.Add(_tileLayer);
        _layers.Add(_attributionLayer);
        InitializeGestures();
    }

    internal void Cleanup()
    {
        _cts.Cancel();
        _cts.Dispose();

        _cts = new();
        foreach (var layer in _layers)
        {
            layer.Dispose();
        }
    }

    private void InitializeGestures()
    {
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(panGesture);

        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += OnPinchUpdated;
        GestureRecognizers.Add(pinchGesture);
    }

    #endregion

    #region Update Handlers

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _lastPanPosition = new PointF((float)e.TotalX, (float)e.TotalY); break;
            case GestureStatus.Running:
                var deltaX = (float)e.TotalX - _lastPanPosition.X;
                var deltaY = (float)e.TotalY - _lastPanPosition.Y;
                _lastPanPosition = new PointF((float)e.TotalX, (float)e.TotalY);

                Center = WebMercatorProjection.Translate(Center, deltaX, deltaY, ZoomLevel);
                Center = WebMercatorProjection.ClampToBounds(Center, ZoomLevel, _zoomScale, _canvasSize.Width,
                    _canvasSize.Height);
                InvalidateSurface();
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
            default:
                _lastPanPosition = PointF.Zero;
                break;
        }
    }

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Running:
                OnScrollZoom((float)e.Scale);
                break;
            case GestureStatus.Started:
                _baseZoomLevel = ZoomLevel;
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
            default:
                break;
        }
    }

    private static void OnMapTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not GoogleTilesView view) return;
        if ((MapType)oldValue == (MapType)newValue) return;

        view._tileLayer.BeginTransition((MapType)newValue, view.InvalidateSurface);
    }

    private static void OnMapThemeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not GoogleTilesView view) return;
        if ((MapTheme)oldValue == (MapTheme)newValue) return;
        var newTheme = (MapTheme)newValue;
        if (view.MapType != MapType.Roadmap && newTheme != MapTheme.Day)
            // Op not allowed, skipping
            return;
        view._tileLayer.BeginTransition(newTheme, view.InvalidateSurface);
    }

    internal void OnScrollZoom(float delta)
    {
        if (_baseZoomLevel == 0)
            _baseZoomLevel = ZoomLevel;
        _accumulatedScale *= delta;
        _accumulatedScale = Math.Clamp(_accumulatedScale,
            Math.Max(Math.Pow(2, WebMercatorProjection.MinZoom - _baseZoomLevel), GetMinScale()),
            Math.Pow(2, WebMercatorProjection.MaxZoom - _baseZoomLevel));

        var trueZoom = _baseZoomLevel + Math.Log2(_accumulatedScale);
        var targetZoomLevel = (int)Math.Floor(trueZoom);
        targetZoomLevel = Math.Clamp(targetZoomLevel,
            WebMercatorProjection.MinZoom, WebMercatorProjection.MaxZoom);
        _zoomScale = Math.Pow(2, trueZoom - targetZoomLevel);

        if (targetZoomLevel != ZoomLevel)
        {
            Debug.WriteLine($"Switching to Zoom Level {targetZoomLevel}");
            ZoomLevel = targetZoomLevel;
            _baseZoomLevel = targetZoomLevel;
            _accumulatedScale = _zoomScale;
        }

        Center = WebMercatorProjection.ClampToBounds(Center, ZoomLevel, _zoomScale, _canvasSize.Width,
            _canvasSize.Height);

        InvalidateSurface();
    }

    #endregion

    #region Helpers

    private static byte[]? LoadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(GoogleTilesView).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private double GetMinScale()
    {
        if (_canvasSize.Height == 0 || _canvasSize.Width == 0)
            return 1.0;
        var worldTiles = 1 << ZoomLevel;
        var worldPixels = worldTiles * WebMercatorProjection.TileSize;

        var minScaleY = _canvasSize.Height / (double)worldPixels;
        var minScaleX = _canvasSize.Width / (double)worldPixels;
        return Math.Max(minScaleX, minScaleY);
    }

    #endregion
}