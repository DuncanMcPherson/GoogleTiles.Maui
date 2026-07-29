using System.Collections.ObjectModel;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Models;

public class Polyline
{
    public ObservableCollection<GeoCoordinate> Positions { get; } = [];
    public Color StrokeColor { get; set; } = Colors.Black;
    public float StrokeWidth { get; set; } = 4f;
    public float[]? DashPattern { get; set; }
    public bool IsClosed { get; set; }
}