using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Extensions;

public static class EnumExtensions
{
    public static string ToApiString(this MapType mapType) => mapType switch
    {
        MapType.Roadmap => "roadmap",
        MapType.Satellite => "satellite",
        MapType.Terrain => "terrain",
        MapType.Streetview => "streetview",
        _ => throw new ArgumentOutOfRangeException(nameof(mapType))
    };

    public static string ToApiString(this ImageFormat imageFormat) => imageFormat switch
    {
        ImageFormat.Png => "png",
        ImageFormat.Jpeg => "jpeg",
        _ => throw new ArgumentOutOfRangeException(nameof(imageFormat))
    };

    public static string ToApiString(this ScaleFactor scaleFactor) => scaleFactor switch
    {
        ScaleFactor.ScaleFactor1x => "scaleFactor1x",
        ScaleFactor.ScaleFactor2x => "scaleFactor2x",
        ScaleFactor.ScaleFactor4x => "scaleFactor4x",
        _ => throw new ArgumentOutOfRangeException(nameof(scaleFactor))
    };
}