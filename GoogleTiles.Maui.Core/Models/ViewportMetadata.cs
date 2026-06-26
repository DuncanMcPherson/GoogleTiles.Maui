namespace GoogleTiles.Maui.Core.Models;

internal class ViewportMetadata
{
    [JsonPropertyName("copyright")] public string Copyright { get; init; } = string.Empty;
    [JsonPropertyName("maxZoomRects")] public IReadOnlyList<MaxZoomRect> MaxZoomRects { get; init; } = [];
}

internal class MaxZoomRect
{
    [JsonPropertyName("maxZoom")]
    public int MaxZoom { get; init; }

    [JsonPropertyName("north")]
    public double North { get; init; }

    [JsonPropertyName("south")]
    public double South { get; init; }

    [JsonPropertyName("east")]
    public double East { get; init; }

    [JsonPropertyName("west")] 
    public double West { get; init; }
}