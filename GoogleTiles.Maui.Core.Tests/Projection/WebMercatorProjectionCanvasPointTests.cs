namespace GoogleTiles.Maui.Core.Tests.Projection;

[TestFixture]
public class WebMercatorProjectionCanvasPointTests
{
    private const int CanvasWidth = 512;
    private const int CanvasHeight = 512;
    private const int Zoom = 10;

    [Test]
    public void Should_ReturnCanvasCenter_When_CoordinateIsCenter()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var result = WebMercatorProjection.ToCanvasPoint(center, center, Zoom, CanvasWidth, CanvasHeight);
        result.X.Should().BeApproximately(CanvasWidth / 2f, 1f);
        result.Y.Should().BeApproximately(CanvasWidth / 2f, 1f);
    }

    [Test]
    public void Should_HaveSmallerY_When_CoordinateIsNorthOfCenter()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var north = new GeoCoordinate(41, -111.8910);
        var result = WebMercatorProjection.ToCanvasPoint(north, center, Zoom, CanvasWidth, CanvasHeight);
        result.Y.Should().BeLessThan(CanvasHeight / 2f);
    }

    [Test]
    public void Should_HaveLargerY_When_CoordinateIsSouthOfCenter()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var south = new GeoCoordinate(40.0, -111.8910);

        var result = WebMercatorProjection.ToCanvasPoint(
            south, center, Zoom, CanvasWidth, CanvasHeight);

        result.Y.Should().BeGreaterThan(CanvasHeight / 2f);
    }

    [Test]
    public void Should_HaveLargerX_When_CoordinateIsEastOfCenter()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var east = new GeoCoordinate(40.7608, -111.0);

        var result = WebMercatorProjection.ToCanvasPoint(
            east, center, Zoom, CanvasWidth, CanvasHeight);

        result.X.Should().BeGreaterThan(CanvasWidth / 2f);
    }

    [Test]
    public void Should_HaveSmallerX_When_CoordinateIsWestOfCenter()
    {
        var center = new GeoCoordinate(40.7608, -111.8910);
        var west = new GeoCoordinate(40.7608, -112.5);

        var result = WebMercatorProjection.ToCanvasPoint(
            west, center, Zoom, CanvasWidth, CanvasHeight);

        result.X.Should().BeLessThan(CanvasWidth / 2f);
    }
}