using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;

namespace GoogleTiles.Maui.Core.Viewport;

internal static class ViewportCalculator
{
    internal static IReadOnlyList<ViewportTile> GetVisibleTiles(
        GeoCoordinate center,
        int zoom,
        int canvasWidth,
        int canvasHeight)
    {
        if (zoom is < WebMercatorProjection.MinZoom or > WebMercatorProjection.MaxZoom)
            throw new ArgumentOutOfRangeException(nameof(zoom), $"Zoom must be between {WebMercatorProjection.MinZoom} and {WebMercatorProjection.MaxZoom}");

        var tileSize = WebMercatorProjection.TileSize;
        var centerTile = WebMercatorProjection.FromLatLng(center.Latitude, center.Longitude, zoom);

        var centerPixelX = Math.Round((center.Longitude + 180.0) / 360.0 * (1 << zoom) * tileSize);
        var centerPixelY = Math.Round((1.0 - (Math.Log(
            Math.Tan(center.Latitude * Math.PI / 180.0) +
            1.0 / Math.Cos(center.Latitude * Math.PI / 180.0)) / Math.PI)) / 2.0 * (1 << zoom) * tileSize);

        // How far into the center tile is our actual center coordinate
        var centerOffsetX = centerPixelX % tileSize;
        var centerOffsetY = centerPixelY % tileSize;


        var tilesX = (int)Math.Ceiling((canvasWidth / 2.0 + centerOffsetX) / tileSize);
        var tilesY = (int)Math.Ceiling((canvasHeight / 2.0 + centerOffsetY) / tileSize);

        var maxTileIndex = (1 << zoom) - 1;
        var worldWidthPixels = (maxTileIndex + 1) * tileSize;
        var worldCopiesNeeded = (int)Math.Ceiling((double)canvasWidth / worldWidthPixels) + 1;
        var totalTilesX = (maxTileIndex + 1) * worldCopiesNeeded;

        var results = new List<ViewportTile>();
        var seen = new HashSet<TileCoordinate>();
        for (var dy = -tilesY; dy <= tilesY; dy++)
        {
            for (var dx = -totalTilesX; dx <= totalTilesX; dx++)
            {
                var tileX = centerTile.X + dx;
                var tileY = centerTile.Y + dy;

                tileX = ((tileX % (maxTileIndex + 1)) + (maxTileIndex + 1)) % (maxTileIndex + 1);
                if (tileY < 0 || tileY > maxTileIndex)
                    continue;

                var rawTileX = centerTile.X + dx;
                var wrappedTileX = ((rawTileX % (maxTileIndex + 1)) + (maxTileIndex + 1)) % (maxTileIndex + 1);

                var pixelX = (float)((wrappedTileX * tileSize) - centerPixelX + canvasWidth / 2f);
                if (rawTileX < 0)
                {
                    pixelX -= worldWidthPixels;
                } else if (rawTileX > maxTileIndex)
                {
                    pixelX += worldWidthPixels;
                }
                else if (pixelX + tileSize - 1 < 0 && pixelX + worldWidthPixels < canvasWidth)
                {
                    pixelX += worldWidthPixels;
                }
                var pixelY = (float)((tileY * tileSize) - centerPixelY + canvasHeight / 2f);
                if (pixelX + tileSize - 1 < 0 ||
                    pixelX >= canvasWidth ||
                    pixelY + tileSize - 1 < 0 ||
                    pixelY >= canvasHeight)
                    continue;


                var tileCoordinate = new TileCoordinate(tileX, tileY, zoom);
                if (!seen.Add(tileCoordinate))
                    continue;
                results.Add(new ViewportTile(
                    tileCoordinate,
                    new TilePixelPosition(pixelX, pixelY)));
            }
        }
        return results;
    }
}