using GoogleTiles.Maui.Core.Models;
using SkiaSharp;

namespace GoogleTiles.Maui.Models;

public record LayerDrawContext(
    GeoCoordinate Center,
    int ZoomLevel,
    double ZoomScale,
    SKSizeI CanvasSize,
    SKMatrix Matrix,
    bool TrackUserLocation,
    float RotationDegrees = 0f);