namespace GoogleTiles.Maui.Core.Tests;

[TestFixture]
public class WebMercatorProjectionTests
{
    #region FromLatLng

    [Test]
    public void FromLatLng_ZoomZer0_ReturnsOriginTile()
    {
        var result = WebMercatorProjection.FromLatLng(0, 0, 0);

        result.X.Should().Be(0);
        result.Y.Should().Be(0);
        result.Zoom.Should().Be(0);
    }

    [Test]
    public void FromLatLng_Tokyo_ZoomOne_ReturnsCorrectTile()
    {
        var result = WebMercatorProjection.FromLatLng(35.6762, 139.6503, 1);
        result.Should().Be(new TileCoordinate(1, 0, 1));
    }

    [Test]
    public void FromLatLng_MinZoom_DoesNotThrow()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, 0, WebMercatorProjection.MinZoom);
        act.Should().NotThrow();
    }

    [Test]
    public void FromLatLng_MaxZoom_DoesNotThrow()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, 0, WebMercatorProjection.MaxZoom);
        act.Should().NotThrow();
    }

    [Test]
    public void FromLatLng_MaxLatitude_DoesNotThrow()
    {
        var act = () => WebMercatorProjection.FromLatLng(85.0511, 0, 1);
        act.Should().NotThrow();
    }

    [Test]
    public void FromLatLng_MinLatitude_DoesNotThrow()
    {
        var act = () => WebMercatorProjection.FromLatLng(-85.0511, 0, 1);
        act.Should().NotThrow();
    }

    [Test]
    public void FromLatLng_ZoomBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, 0, WebMercatorProjection.MinZoom - 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromLatLng_ZoomAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, 0, WebMercatorProjection.MaxZoom + 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromLatLng_LatitudeAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(85.0512, 0, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromLatLng_LatitudeBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(-85.0512, 0, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromLatLng_LongitudeAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, 180.1, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void FromLatLng_LongitudeBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.FromLatLng(0, -180.1, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region ToLatLng

    [Test]
    public void ToLatLng_ZoomZeroOrigin_ReturnsTopLeftCorner()
    {
        var (lat, lng) = WebMercatorProjection.ToLatLng(new TileCoordinate(0, 0, 0));
        lat.Should().BeApproximately(85.0511, 0.0001);
        lng.Should().BeApproximately(-180.0, 0.0001);
    }

    [Test]
    public void ToLatLng_ZoomBelowMin_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.ToLatLng(new TileCoordinate(0, 0, WebMercatorProjection.MinZoom - 1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ToLatLng_ZoomAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => WebMercatorProjection.ToLatLng(new TileCoordinate(0, 0, WebMercatorProjection.MaxZoom + 1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Round-trip

    [TestCase(51.5, -0.1, 10)]   // London
    [TestCase(40.7128, -74.006, 12)]  // New York
    [TestCase(35.6762, 139.6503, 8)]  // Tokyo
    [TestCase(0, 0, 5)]               // Null Island
    public void RoundTrip_FromLatLngThenToLatLng_ReturnsApproximateOrigin(
        double latitude, double longitude, int zoom)
    {
        var tile = WebMercatorProjection.FromLatLng(latitude, longitude, zoom);
        var (resultLat, resultLng) = WebMercatorProjection.ToLatLng(tile);

        // ToLatLng returns top-left corner of tile, so we allow for tile width/height tolerance
        var tolerance = 360.0 / (1 << zoom);
        resultLat.Should().BeApproximately(latitude, tolerance);
        resultLng.Should().BeApproximately(longitude, tolerance);
    }

    #endregion
}