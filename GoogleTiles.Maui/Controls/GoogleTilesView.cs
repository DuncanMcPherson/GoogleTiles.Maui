using GoogleTiles.Maui.Models;

namespace GoogleTiles.Maui.Controls;

public class GoogleTilesView : View
{
    #region Bindable Properties

    public static readonly BindableProperty CenterProperty = BindableProperty.Create(
        nameof(Center),
        typeof(GeoCoordinate),
        typeof(GoogleTilesView),
        new GeoCoordinate(0, 0));

    public static readonly BindableProperty ZoomLevelProperty = BindableProperty.Create(
        nameof(ZoomLevel),
        typeof(int),
        typeof(GoogleTilesView),
        15);

    public GeoCoordinate Center
    {
        get => (GeoCoordinate)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public int ZoomLevel
    {
        get => (int)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    #endregion
}