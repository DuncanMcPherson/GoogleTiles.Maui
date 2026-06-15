using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Projection;

public static class WebMercatorProjection
{
    public const int TileSize = 256;
    public const int MinZoom = 0;
    public const int MaxZoom = 22;

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