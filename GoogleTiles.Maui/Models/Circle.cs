using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Models;

public class Circle
{
    public GeoCoordinate Position { get; set; }
    public float RadiusInFeet { get; set; }
    public float StrokeWidth { get; set; } = 4f;
    public Color? EdgeColor { get; set; }
    public Color? FillColor { get; set; }
    public string? Tag { get; set; }
}