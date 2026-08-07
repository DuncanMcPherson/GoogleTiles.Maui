using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Models;

public class Polygon
{
    public List<GeoCoordinate> Positions { get; set; } = [];
    public float StrokeWidth { get; set; } = 4f;
    public Color? EdgeColor { get; set; }
    public Color? FillColor { get; set; }
}