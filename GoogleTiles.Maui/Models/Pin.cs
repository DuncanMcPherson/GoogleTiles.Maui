using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Models;

/// <summary>
/// A pinned location on the map
/// </summary>
/// <param name="Location">The geographic coordinates of the pin</param>
/// <param name="ImageSource">Optional source for a custom graphic</param>
/// <param name="Label">A label or name for a pin. This is only displayed if <see cref="ShowLabel"/> is true and can be used for identifying pins programmatically</param>
/// <param name="Scale">The scale of the image used</param>
/// <param name="Rotation">Rotation in degrees</param>
public record Pin(
    GeoCoordinate Location,
    ImageSource? ImageSource = null,
    string? Label = null,
    bool ShowLabel = false,
    float Scale = 1f,
    float Rotation = 0f);