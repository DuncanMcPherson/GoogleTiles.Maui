using System.ComponentModel;
using System.Globalization;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Converters;

public class GeoCoordinateTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            var parts = str.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
                return new GeoCoordinate(lat, lng);
            throw new InvalidOperationException(
                $"Cannot convert '{str}' to GeoCoordinate. Expected format: 'latitude, longitude'");
        }

        return base.ConvertFrom(context, culture, value);
    }
}