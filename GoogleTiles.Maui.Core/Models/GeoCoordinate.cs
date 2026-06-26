using System.ComponentModel;
using GoogleTiles.Maui.Core.Converters;

namespace GoogleTiles.Maui.Core.Models;

[TypeConverter(typeof(GeoCoordinateTypeConverter))]
public record struct GeoCoordinate(double Latitude, double Longitude);