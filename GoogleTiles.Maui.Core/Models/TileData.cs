namespace GoogleTiles.Maui.Core.Models;

internal record TileData(
    byte[] Bytes,
    string ContentType,
    TimeSpan? MaxAge,
    bool NoStore);