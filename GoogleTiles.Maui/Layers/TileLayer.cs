using System.Collections.Concurrent;
using System.Diagnostics;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Core.Viewport;
using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

internal class TileLayer : MapLayer
{
    private readonly TileFetcher _tileFetcher;
    private readonly SessionTokenCache _sessionTokenCache;
    private readonly GoogleTilesOptions _options;
    private readonly ConcurrentDictionary<TileCoordinate, CachedTile?> _tileCache = new();
    private readonly HashSet<TileCoordinate> _pendingFetches = new();
    private readonly object _pendingLock = new();

    private float _transitionAlpha = 1.0f;
    private bool _isTransitioning;
    private IDispatcherTimer? _transitionTimer;

    public TileLayer(
        string id,
        TileFetcher tileFetcher,
        SessionTokenCache sessionTokenCache,
        GoogleTilesOptions options)
        : base(id)
    {
        _tileFetcher = tileFetcher;
        _sessionTokenCache = sessionTokenCache;
        _options = options;
    }

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        var visibleTiles = ViewportCalculator.GetVisibleTiles(
            context.Center,
            context.ZoomLevel,
            context.CanvasSize.Width,
            context.CanvasSize.Height);

        // if (context.ZoomScale != 1.0)
        // {
        //     canvas.Save();
        //     canvas.Scale((float)context.ZoomScale,
        //         (float)context.ZoomScale,
        //         context.CanvasSize.Width / 2f,
        //         context.CanvasSize.Height / 2f);
        // }

        using var paint = _isTransitioning
            ? new SKPaint { Color = SKColors.White.WithAlpha((byte)(_transitionAlpha * 255)) }
            : null;

        foreach (var viewportTile in visibleTiles)
        {
            if (_tileCache.TryGetValue(viewportTile.Coordinate, out var cachedTile))
            {
                // Tile fetch has been requested and is pending
                if (cachedTile is null) continue;

                // Cache entry has expired per Google's Cache-Control header, refresh
                if (cachedTile.IsExpired)
                {
                    _tileCache.TryRemove(viewportTile.Coordinate, out _);
                    QueueTileFetch(viewportTile.Coordinate, context);
                    continue;
                }

                // Mark the tile as having been recently used, then draw it with applicable alpha
                cachedTile.Touch();
                canvas.DrawBitmap(cachedTile.Bitmap,
                    new SKPoint(viewportTile.PixelPosition.X, viewportTile.PixelPosition.Y),
                    paint);
            }
            else
            {
                // Create the cache entry, then request the tile from the server
                _tileCache[viewportTile.Coordinate] = null;
                QueueTileFetch(viewportTile.Coordinate, context);
            }
        }
    }

    public void BeginTransition(MapTheme newTheme, Action invalidateSurface)
    {
        _options.Theme = newTheme;
        _sessionTokenCache.Clear();

        foreach (var tile in _tileCache.Values)
            tile?.Bitmap.Dispose();
        _tileCache.Clear();

        // Set to beginning of animation
        _transitionAlpha = 0f;
        _isTransitioning = true;

        // Create a clean timer
        _transitionTimer?.Stop();
        _transitionTimer = Application.Current!.Dispatcher.CreateTimer();
        _transitionTimer.Interval = TimeSpan.FromMilliseconds(16);
        _transitionTimer.Tick += (_, _) =>
        {
            // Update alpha and trigger repaint
            _transitionAlpha = Math.Min(_transitionAlpha + 0.05f, 1.0f);
            MainThread.BeginInvokeOnMainThread(invalidateSurface);

            // If alpha is 1 or higher, stop and clear the timer
            if (_transitionAlpha >= 1.0f)
            {
                _isTransitioning = false;
                _transitionTimer.Stop();
                _transitionTimer = null;
            }
        };
        // Start the animation
        _transitionTimer.Start();
    }

    public void BeginTransition(MapType newMapType, Action invalidateSurface)
    {
        // Store new map type
        _options.MapType = newMapType;
        // Clear session token
        _sessionTokenCache.Clear();

        // Dispose active tiles
        foreach (var tile in _tileCache.Values)
            tile?.Bitmap.Dispose();
        _tileCache.Clear();

        // Set to beginning of animation
        _transitionAlpha = 0f;
        _isTransitioning = true;

        // Create a clean timer
        _transitionTimer?.Stop();
        _transitionTimer = Application.Current!.Dispatcher.CreateTimer();
        _transitionTimer.Interval = TimeSpan.FromMilliseconds(16);
        _transitionTimer.Tick += (_, _) =>
        {
            // Update alpha and trigger repaint
            _transitionAlpha = Math.Min(_transitionAlpha + 0.05f, 1.0f);
            MainThread.BeginInvokeOnMainThread(invalidateSurface);

            // If alpha is 1 or higher, stop and clear the timer
            if (_transitionAlpha >= 1.0f)
            {
                _isTransitioning = false;
                _transitionTimer.Stop();
                _transitionTimer = null;
            }
        };
        // Start the animation
        _transitionTimer.Start();
    }

    public override void Dispose()
    {
        _transitionTimer?.Stop();
        foreach (var tile in _tileCache.Values)
            tile?.Bitmap.Dispose();
        _tileCache.Clear();
    }

    private void QueueTileFetch(TileCoordinate coordinate, LayerDrawContext context)
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

                MainThread.BeginInvokeOnMainThread(() => Application.Current?.Dispatcher.Dispatch(RequestRepaint));
            }
        });
    }

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
            if (_tileCache.TryGetValue(key, out var tile) && tile?.IsExpired == true)
                _tileCache.TryRemove(key, out _);

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
}