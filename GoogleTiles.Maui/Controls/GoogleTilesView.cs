using System.Collections.Concurrent;
using System.Diagnostics;
using GoogleTiles.Maui.Attribution;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Core.Viewport;
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
    private GoogleTilesOptions _options;
    private ViewportMetadataFetcher _viewportMetadataFetcher;
    private readonly ConcurrentDictionary<TileCoordinate, CachedTile?> _tileCache = new();
    private readonly HashSet<TileCoordinate> _pendingFetches = new();
    private readonly object _pendingLock = new();
    private PointF _lastPanPosition;
    private double _accumulatedScale = 1.0;
    private double _zoomScale = 1.0;
    private int _baseZoomLevel;
    private string _copyrightText = "Map data ©Google Maps";
    private AttributionOverlay _attributionOverlay;
    private GeoCoordinate _lastMetadataCenter;
    private int _lastMetadataZoom;

    private readonly SemaphoreSlim _drawLock = new(1, 1);
    private CancellationTokenSource _cts = new();

    private void StoreTile(TileCoordinate coordinate, TileData tileData, SKBitmap bitmap)
    {
        if (!_options.EnableCaching || tileData.NoStore)
        {
            _tileCache.TryRemove(coordinate, out _);
            return;
        }

        var expiresAt = tileData.MaxAge.HasValue
            ? DateTimeOffset.UtcNow.Add(tileData.MaxAge.Value)
            : (DateTimeOffset?)null;
        _tileCache[coordinate] = new CachedTile(bitmap, expiresAt);
        EvictIfNecessary();
    }

    private void EvictIfNecessary()
    {
        if (_tileCache.Count <= _options.MaxCachedTiles)
            return;

        foreach (var key in _tileCache.Keys)
        {
            if (_tileCache.TryGetValue(key, out var tile) && tile?.IsExpired == true)
                _tileCache.TryRemove(key, out _);
        }

        if (_tileCache.Count <= _options.MaxCachedTiles)
            return;

        var toEvict = _tileCache
            .Where(kvp => kvp.Value is not null)
            .OrderBy(kvp => kvp.Value!.LastAccessed)
            .Take(_tileCache.Count - _options.MaxCachedTiles)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach (var key in toEvict)
            _tileCache.TryRemove(key, out _);
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

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        if (_tileFetcher is null || _options is null)
            return;

        if (!_drawLock.Wait(0))
            return;
        try
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            _canvasSize = e.Info.Size;

            var visibleTiles =
                ViewportCalculator.GetVisibleTiles(Center, ZoomLevel, _canvasSize.Width, _canvasSize.Height);

            foreach (var viewportTile in visibleTiles)
            {
                if (_tileCache.TryGetValue(viewportTile.Coordinate, out var cachedTile))
                {
                    if (cachedTile is null)
                        continue; // Fetch in progress

                    if (cachedTile.IsExpired)
                    {
                        _tileCache.TryRemove(viewportTile.Coordinate, out _);
                        QueueTileFetch(viewportTile.Coordinate);
                        continue;
                    }

                    cachedTile.Touch();
                    DrawTile(canvas, cachedTile.Bitmap, viewportTile.PixelPosition);
                }
                else
                {
                    _tileCache[viewportTile.Coordinate] = null;
                    QueueTileFetch(viewportTile.Coordinate);
                }
            }

            if (Center != _lastMetadataCenter || ZoomLevel != _lastMetadataZoom)
            {
                _lastMetadataCenter = Center;
                _lastMetadataZoom = ZoomLevel;
                DrawAttribution(canvas, visibleTiles);
            }
        }
        finally
        {
            _drawLock.Release();
        }
    }

    private void DrawTile(SKCanvas canvas, SKBitmap bitmap, TilePixelPosition position)
    {
        if (_zoomScale != 1.0)
        {
            canvas.Save();
            _zoomScale = Math.Max(_zoomScale, GetMinScale());
            canvas.Scale((float)_zoomScale, (float)_zoomScale,
                _canvasSize.Width / 2f,
                _canvasSize.Height / 2f);
            canvas.DrawBitmap(bitmap, new SKPoint(position.X, position.Y));
            canvas.Restore();
        }
        else
            canvas.DrawBitmap(bitmap, new SKPoint(position.X, position.Y));
    }

    private void QueueTileFetch(TileCoordinate coordinate)
    {
        lock (_pendingLock)
        {
            if (!_pendingFetches.Add(coordinate))
                return;
        }

        Task.Run(async () =>
        {
            try
            {
                var tileData = await _tileFetcher.FetchAsync(coordinate);
                var bitmap = SKBitmap.Decode(tileData.Bytes);
                StoreTile(coordinate, tileData, bitmap);
            }
            catch (Exception ex)
            {
                _tileCache.TryRemove(coordinate, out _);
                Debug.WriteLine($"Tile fetch failed for ({coordinate.X}, {coordinate.Y}): {ex.Message}");
            }
            finally
            {
                lock (_pendingLock)
                    _pendingFetches.Remove(coordinate);

                MainThread.BeginInvokeOnMainThread(InvalidateSurface);
            }
        });
    }

    private void DrawAttribution(SKCanvas canvas, IReadOnlyList<ViewportTile> visibleTiles)
    {
        var (north, south, east, west) = WebMercatorProjection.GetViewportBounds(visibleTiles);
        Task.Run(async () =>
        {
            var metadata = await _viewportMetadataFetcher.FetchAsync(ZoomLevel, north, south, east, west);

            if (metadata is not null && !string.IsNullOrEmpty(metadata.Copyright) &&
                metadata.Copyright != _copyrightText)
            {
                _copyrightText = metadata.Copyright;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // _attributionOverlay.Draw(canvas, _canvasSize.Width, _canvasSize.Height, _copyrightText);
                    InvalidateSurface();
                });
            }
        });
    }

    internal void Initialize(TileFetcher tileFetcher, ISessionTokenProvider sessionTokenProvider,
        GoogleTilesOptions options, ViewportMetadataFetcher metadataFetcher)
    {
        _tileFetcher = tileFetcher;
        _sessionTokenProvider = sessionTokenProvider;
        _options = options;
        _viewportMetadataFetcher = metadataFetcher;
        _attributionOverlay = new AttributionOverlay();
        var logoBytes = LoadEmbeddedResource("GoogleTiles.Maui.Resources.googlemaps_logo_withdarkoutline_1x.png");
        _attributionOverlay.LoadLogo(logoBytes);
        InitializeGestures();
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
                _accumulatedScale *= e.Scale;
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

    private static byte[]? LoadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(GoogleTilesView).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    internal void Cleanup()
    {
        _cts.Cancel();
        _cts.Dispose();

        _cts = new();
        foreach (var tile in _tileCache.Values)
            tile?.Bitmap.Dispose();

        _tileCache.Clear();
    }
}