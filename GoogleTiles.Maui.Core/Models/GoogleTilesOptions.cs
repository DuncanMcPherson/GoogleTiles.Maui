namespace GoogleTiles.Maui.Core.Models;

public class GoogleTilesOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public bool EnableCaching { get; set; } = false;
    public MapType MapType { get; set; } = MapType.Roadmap;
    public string Language { get; set; } = "en-US";
    public string Region { get; set; } = "US";
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;
    public ScaleFactor Scale { get; set; } = ScaleFactor.ScaleFactor1x;
    public bool HighDpi { get; set; } = false;
}

public enum MapType { Roadmap, Satellite, Terrain, Streetview }
public enum ImageFormat { Png, Jpeg }
public enum ScaleFactor { ScaleFactor1x, ScaleFactor2x, ScaleFactor4x }