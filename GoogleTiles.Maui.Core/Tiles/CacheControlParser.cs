using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Tiles;

internal static class CacheControlParser
{
    /// <summary>
    /// Parses a Cache-Control header value and returns a TileData object with the appropriate caching directives.
    ///
    /// If the header contains "no-store", is null or empty, or is invalid, the returned TileData will have NoStore set to true and MaxAge set to null.
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public static TileData Parse(string? header)
    {
        if (string.IsNullOrEmpty(header))
            return new TileData([], string.Empty, null, true);

        var directives = header
            .Split(',')
            .Select(d => d.Trim().ToLowerInvariant())
            .ToList();

        if (directives.Contains("no-store"))
            return new TileData([], string.Empty, null, true);

        if (directives.Contains("no-cache"))
            return new TileData([], string.Empty, null, false);

        var maxAgeDirective = directives.FirstOrDefault(d => d.StartsWith("max-age="));

        if (maxAgeDirective is not null)
        {
            var valueStr = maxAgeDirective["max-age=".Length..];
            if (long.TryParse(valueStr, out var seconds))
                return new TileData([], string.Empty, TimeSpan.FromSeconds(seconds), false);
        }

        return new TileData([], string.Empty, null, true);
    }
}