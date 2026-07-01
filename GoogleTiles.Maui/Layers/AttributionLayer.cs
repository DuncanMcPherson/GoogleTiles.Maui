using System.Diagnostics;
using GoogleTiles.Maui.Attribution;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Core.Viewport;
using GoogleTiles.Maui.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Layers;

internal class AttributionLayer : MapLayer
{
    private readonly ViewportMetadataFetcher _metadataFetcher;
    private readonly AttributionOverlay _overlay;
    private string _copyrightText = "Map data ©Google Maps";
    private GeoCoordinate _lastCenter;
    private int _lastZoom;

    public AttributionLayer(
        string id,
        ViewportMetadataFetcher metadataFetcher,
        byte[]? logoBytes) : base(id)
    {
        _metadataFetcher = metadataFetcher;
        _overlay = new AttributionOverlay();
        _overlay.LoadLogo(logoBytes);
    }

    protected override void Draw(SKCanvas canvas, SKImageInfo info, LayerDrawContext context)
    {
        Debug.WriteLine("***Attribution draw***");
        _overlay.Draw(canvas, context.CanvasSize.Width, context.CanvasSize.Height, _copyrightText);

        if (context.Center == _lastCenter && context.ZoomLevel == _lastZoom)
            return;

        _lastCenter = context.Center;
        _lastZoom = context.ZoomLevel;

        var visibleTiles = ViewportCalculator.GetVisibleTiles(context.Center, context.ZoomLevel,
            context.CanvasSize.Width, context.CanvasSize.Height);

        var (north, south, east, west) = WebMercatorProjection.GetViewportBounds(visibleTiles);

        Task.Run(async () =>
        {
            Debug.WriteLine($"Queueing Metadata Fetch for Zoom: {context.ZoomLevel}, width: {context.CanvasSize.Width}, height: {context.CanvasSize.Height}");
            var metadata = await _metadataFetcher.FetchAsync(context.ZoomLevel, north, south, east, west);
            Debug.WriteLine($"Metadata fetch complete: {(metadata is not null ? "Success" : "Fail")}");
            if (metadata is not null &&
                !string.IsNullOrEmpty(metadata.Copyright) &&
                metadata.Copyright != _copyrightText)
            {
                _copyrightText = metadata.Copyright;
                MainThread.BeginInvokeOnMainThread(RequestRepaint);
            }
        });
    }

    public override void Dispose()
    {
        // No op
    }
}