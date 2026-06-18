using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Viewport;

internal record struct TilePixelPosition(float X, float Y);

internal record ViewportTile(TileCoordinate Coordinate, TilePixelPosition PixelPosition);