using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Viewport;

namespace GoogleTiles.Maui.Core.Projection;

internal static class WebMercatorProjection
{
    public const int TileSize = 256;
    public const int MinZoom = 0;
    public const int MaxZoom = 22;

    public static TilePixelPosition ToCanvasPoint(
        GeoCoordinate coordinate,
        GeoCoordinate center,
        int zoom,
        double zoomScale,
        int canvasWidth,
        int canvasHeight)
    {
        ValidateZoom(zoom);
        var worldSize = (1 << zoom) * TileSize;

        var pixelX = (coordinate.Longitude + 180) / 360.0 * worldSize;
        var pixelY =
            (1.0 - Math.Log(Math.Tan(coordinate.Latitude * Math.PI / 180) +
                             1.0 / Math.Cos(coordinate.Latitude * Math.PI / 180)) / Math.PI) / 2.0 * worldSize;

        var centerPixelX = (center.Longitude + 180.0) / 360.0 * worldSize;
        var centerPixelY = (1.0 - (Math.Log(
            Math.Tan(center.Latitude * Math.PI / 180.0) +
            1.0 / Math.Cos(center.Latitude * Math.PI / 180.0)) / Math.PI)) / 2.0 * worldSize;

        var offsetX = (pixelX - centerPixelX) * zoomScale;
        var offsetY = (pixelY - centerPixelY) * zoomScale;

        var canvasX = (float)(canvasWidth / 2.0 + offsetX);
        var canvasY = (float)(canvasHeight / 2.0 + offsetY);

        return new TilePixelPosition(canvasX, canvasY);
    }

    public static GeoCoordinate Translate(
        GeoCoordinate center,
        float pixelDeltaX,
        float pixelDeltaY,
        int zoom)
    {
        ValidateZoom(zoom);

        var tileCound = 1 << zoom;
        var worldSize = tileCound * TileSize;

        var centerPixelX = (center.Longitude + 180) / 360.0 * worldSize;
        var centerPixelY = (1.0 - (Math.Log(
            Math.Tan(center.Latitude * Math.PI / 180.0) +
            1.0 / Math.Cos(center.Latitude * Math.PI / 180)) / Math.PI)) / 2.0 * worldSize;

        var newPixelX = centerPixelX - pixelDeltaX;
        var newPixelY = centerPixelY - pixelDeltaY;

        newPixelX = ((newPixelX % worldSize) + worldSize) % worldSize;
        newPixelY = Math.Clamp(newPixelY, 0, worldSize);

        var longitude = newPixelX / worldSize * 360 - 180;
        var latRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * newPixelY / worldSize)));
        var latitude = latRad * 180 / Math.PI;
        latitude = Math.Clamp(latitude, -85.0511, 85.0511);

        return new GeoCoordinate(latitude, longitude);
    }

    internal static GeoCoordinate ClampToBounds(
        GeoCoordinate center,
        int zoom,
        double zoomScale,
        int canvasWidth,
        int canvasHeight)
    {
        var tileCount = 1 << zoom;
        var worldPixels = tileCount * TileSize * zoomScale;

        if (worldPixels <= canvasHeight)
            return center with { Latitude = 0 };

        var centerPixelY = (1.0 - (Math.Log(
            Math.Tan(center.Latitude * Math.PI / 180.0) +
            1.0 / Math.Cos(center.Latitude * Math.PI / 180.0)) / Math.PI)) / 2.0 * worldPixels;
        var minCenterPixelY = canvasHeight / 2.0;
        var maxCenterPixelY = worldPixels - canvasHeight / 2.0;
        centerPixelY = Math.Clamp(centerPixelY, minCenterPixelY, maxCenterPixelY);

        var latRad = Math.Atan(Math.Sinh(Math.PI * (1.0 - 2.0 * centerPixelY / worldPixels)));
        var latitude = Math.Clamp(latRad * 180 / Math.PI, -85.0511, 85.0511);
        return center with { Latitude = latitude };
    }

    public static (double Latitude, double Longitude) ToLatLngCenter(TileCoordinate tile)
    {
        var (topLat, leftLng) = ToLatLng(tile);
        var (bottomLat, rightLng) = ToLatLng(new TileCoordinate(tile.X + 1, tile.Y + 1, tile.Zoom));

        return ((topLat + bottomLat) / 2.0, (leftLng + rightLng) / 2.0);
    }

    public static TileCoordinate FromLatLng(double latitude, double longitude, int zoom)
    {
        ValidateZoom(zoom);
        ValidateLatLng(latitude, longitude);

        var x = (int)Math.Floor((longitude + 180.0) / 360.0 * (1 << zoom));
        var latRad = latitude * Math.PI / 180.0;
        var y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 *
                                (1 << zoom));
        return new TileCoordinate(x, y, zoom);
    }

    public static (double Latitude, double Longitude) ToLatLng(TileCoordinate tile)
    {
        ValidateZoom(tile.Zoom);

        var n = Math.PI - 2.0 * Math.PI * tile.Y / (1 << tile.Zoom);
        var latitude = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
        var longitude = tile.X / (double)(1 << tile.Zoom) * 360.0 - 180.0;

        return (latitude, longitude);
    }

    internal static (double North, double South, double East, double West) GetViewportBounds(
        IReadOnlyList<ViewportTile> visibleTiles)
    {
        var north = double.MinValue;
        var south = double.MaxValue;
        var east = double.MinValue;
        var west = double.MaxValue;

        foreach (var tile in visibleTiles)
        {
            var (tileLat, tileLng) = ToLatLng(tile.Coordinate);
            var (nextLat, nextLng) =
                ToLatLng(new TileCoordinate(tile.Coordinate.X + 1, tile.Coordinate.Y + 1, tile.Coordinate.Zoom));

            north = Math.Max(north, tileLat);
            south = Math.Min(south, nextLat);
            east = Math.Max(east, nextLng);
            west = Math.Min(west, tileLng);
        }

        return (north, south, east, west);
    }

    private static void ValidateZoom(int zoom)
    {
        if (zoom < MinZoom || zoom > MaxZoom)
            throw new ArgumentOutOfRangeException(nameof(zoom), $"Zoom must be between {MinZoom} and {MaxZoom}.");
    }

    private static void ValidateLatLng(double latitude, double longitude)
    {
        if (latitude < -85.0511 || latitude > 85.0511)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -85.0511 and 85.0511.");
        if (longitude < -180.0 || longitude > 180.0)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180.0 and 180.0.");
    }
}