namespace GoogleTiles.Maui.Core.Tests.Projection;

[TestFixture]
public class WebMercatorProjectionTranslateTests
{
    [Test]
    public void Should_MoveEast_When_PositiveDeltaX()
    {
        var center = new GeoCoordinate(0, 0);
        var result = WebMercatorProjection.Translate(center, 100, 0, 10);
        result.Longitude.Should().BeLessThan(center.Longitude);
    }

    [Test]
    public void Should_MoveWest_When_NegativeDeltaX()
    {
        var center = new GeoCoordinate(0, 0);
        var result = WebMercatorProjection.Translate(center, -100, 0, 10);
        result.Longitude.Should().BeGreaterThan(center.Longitude);
    }

    [Test]
    public void Should_MoveSouth_When_PositiveDeltaY()
    {
        var center = new GeoCoordinate(0, 0);
        var result = WebMercatorProjection.Translate(center, 0, 100, 10);
        result.Latitude.Should().BeGreaterThan(center.Latitude);
    }

    [Test]
    public void Should_MoveNorth_When_NegativeYDelta()
    {
        var center = new GeoCoordinate(0, 0);
        var result = WebMercatorProjection.Translate(center, 0, -100, 10);
        result.Latitude.Should().BeLessThan(center.Latitude);
    }

    [Test]
    public void Should_ReturnSameCenter_When_ZeroDelta()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var result = WebMercatorProjection.Translate(center, 0, 0, 10);
        result.Latitude.Should().BeApproximately(center.Latitude, 0.0001);
        result.Longitude.Should().BeApproximately(center.Longitude, 0.0001);
    }

    [Test]
    public void Should_WrapLongitude_When_CrossingAntimeridian()
    {
        var center = new GeoCoordinate(0, 179.9);
        var result = WebMercatorProjection.Translate(center, -500, 0, 10);
        result.Longitude.Should().BeGreaterThan(-180);
        result.Longitude.Should().BeLessThanOrEqualTo(180);
    }

    [Test]
    public void Should_ClampToNorthBound_When_PanningBeyondNorthPole()
    {
        var center = new GeoCoordinate(84, 0);
        var result = WebMercatorProjection.Translate(center, 0, -1000, 10);
        result.Latitude.Should().BeLessThanOrEqualTo(85.0511);
    }

    [Test]
    public void Should_ClampToSouthBound_When_PanningBeyondSouthPole()
    {
        var center = new GeoCoordinate(-84, 0);
        var result = WebMercatorProjection.Translate(center, 0, 1000, 10);
        result.Latitude.Should().BeGreaterThanOrEqualTo(-85.0511);
    }

    [TestCase(10, 100)]
    [TestCase(15, 100)]
    public void Should_ProduceSmallerDelta_When_ZoomIsHigher(int zoom, float pixelDelta)
    {
        var center = new GeoCoordinate(0, 0);
        var lowZoom = WebMercatorProjection.Translate(center, pixelDelta, 0, zoom);
        var highZoom = WebMercatorProjection.Translate(center, pixelDelta, 0, zoom + 1);

        var lowDelta = Math.Abs(lowZoom.Longitude - center.Longitude);
        var highDelta = Math.Abs(highZoom.Longitude - center.Longitude);

        highDelta.Should().BeLessThan(lowDelta);
    }
}