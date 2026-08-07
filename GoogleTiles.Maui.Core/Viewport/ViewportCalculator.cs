using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Projection;

namespace GoogleTiles.Maui.Core.Viewport;

internal static class ViewportCalculator
{
    /*
     * Known Issues:
     * - Map Corners disappear when map is rotated and tile is near the extremities
     */
    internal static IReadOnlyList<ViewportTile> GetVisibleTiles(
        GeoCoordinate center,
        int zoom,
        int canvasWidth,
        int canvasHeight,
        float rotationDegrees = 0f)
    {
        if (zoom is < WebMercatorProjection.MinZoom or > WebMercatorProjection.MaxZoom)
            throw new ArgumentOutOfRangeException(nameof(zoom));

        var tileSize = WebMercatorProjection.TileSize;
        var maxTileIndex = (1 << zoom) - 1;
        var worldWidthPixels = (maxTileIndex + 1) * tileSize;

        // ------------------------------------------------------------
        // 1. Compute rotated bounding box (FETCH bounds only)
        // ------------------------------------------------------------
        int fetchWidth, fetchHeight;

        if (rotationDegrees == 0f)
        {
            fetchWidth = canvasWidth;
            fetchHeight = canvasHeight;
        }
        else
        {
            var radians = Math.Abs(rotationDegrees * Math.PI / 180.0);
            var sin = Math.Abs(Math.Sin(radians));
            var cos = Math.Abs(Math.Cos(radians));

            fetchWidth = (int)Math.Ceiling(canvasWidth * cos + canvasHeight * sin);
            fetchHeight = (int)Math.Ceiling(canvasWidth * sin + canvasHeight * cos);
        }

        // ------------------------------------------------------------
        // 2. Compute world pixel coordinates of the map center
        // ------------------------------------------------------------
        var centerPixelX = (center.Longitude + 180.0) / 360.0 * worldWidthPixels;
        var centerPixelY =
            (1.0 - Math.Log(
                Math.Tan(center.Latitude * Math.PI / 180.0) +
                1.0 / Math.Cos(center.Latitude * Math.PI / 180.0)) / Math.PI)
            / 2.0 * worldWidthPixels;
        var centerTile = WebMercatorProjection.FromLatLng(center.Latitude, center.Longitude, zoom);

        // Offset inside the center tile
        var centerOffsetY = (int)(centerPixelY % tileSize);

        // ------------------------------------------------------------
        // 3. Determine how many tiles we need (FETCH bounds)
        // ------------------------------------------------------------
        var tilesY = (int)Math.Ceiling((fetchHeight / 2.0 + centerOffsetY) / tileSize);

        //World Wrap copies
        var worldCopiesNeeded = (int)Math.Ceiling((double)fetchWidth / worldWidthPixels);
        var totalTilesX = (maxTileIndex + 1) * worldCopiesNeeded;

        // ------------------------------------------------------------
        // 4. Compute DRAW bounds (actual screen)
        // ------------------------------------------------------------
        var drawLeft = 0f - (2 * tileSize);
        var drawTop = 0f - (2 * tileSize);
        var drawRight = canvasWidth + (2 * tileSize);
        var drawBottom = canvasHeight + (2 * tileSize);

        // ------------------------------------------------------------
        // 5. Generate visible tiles
        // ------------------------------------------------------------
        var results = new List<ViewportTile>();
        var seen = new HashSet<TileCoordinate>();

        for (var dy = -tilesY; dy <= tilesY; dy++)
        {
            for (var dx = -totalTilesX; dx <= totalTilesX; dx++)
            {
                var rawTileX = centerTile.X + dx;
                var tileY = centerTile.Y + dy;

                if (tileY < 0 || tileY > maxTileIndex)
                    continue;

                var wrappedTileX = ((rawTileX % (maxTileIndex + 1)) + (maxTileIndex + 1)) % (maxTileIndex + 1);

                var pixelX = (float)((wrappedTileX * tileSize) - centerPixelX + canvasWidth / 2f);
                var pixelY = (float)((tileY * tileSize) - centerPixelY + canvasHeight / 2f);

                if (rawTileX < 0)
                    pixelX -= worldWidthPixels;
                else if (rawTileX > maxTileIndex)
                    pixelX += worldWidthPixels;

                if (pixelX + tileSize < drawLeft ||
                    pixelX > drawRight ||
                    pixelY + tileSize < drawTop ||
                    pixelY > drawBottom)
                    continue;

                var tileCoord = new TileCoordinate(wrappedTileX, tileY, zoom);
                if (!seen.Add(tileCoord))
                    continue;

                results.Add(new ViewportTile(
                    tileCoord,
                    new TilePixelPosition(pixelX, pixelY)));
            }
        }

        return results;
    }
}