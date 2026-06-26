using GoogleTiles.Maui.Core.Viewport;

namespace GoogleTiles.Maui.Core.Tests.Viewport;

[TestFixture]
public class ViewportCalculatorTests
{
    private const int SmallCanvas = 512;
    private const int MediumCanvas = 768;


    [Test]
    public void Should_AlwaysIncludeCenterTile_When_Called()
    {
        var center = new GeoCoordinate(51.5, -0.1);
        var result = ViewportCalculator.GetVisibleTiles(
            center,
            10,
            SmallCanvas,
            SmallCanvas);

        var centerTile = WebMercatorProjection.FromLatLng(center.Latitude, center.Longitude, 10);
        result.Should().Contain(t => t.Coordinate == centerTile);
    }

    [Test]
    public void Should_ReturnFourTiles_When_CanvasIsExactlyTwoByTwo()
    {
        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(0, 0),
            3,
            SmallCanvas,
            SmallCanvas);

        result.Should().HaveCount(4);
    }

    [Test]
    public void GetVisibleTiles_ThreeByThreeCanvas_OffsetCoordinate_ReturnsSixteenTiles()
    {
        // 768x768 canvas — exactly 3x3 tiles of 256px each
        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(40.7128, -74.0060),
            zoom: 10,
            canvasWidth: MediumCanvas,
            canvasHeight: MediumCanvas);

        result.Should().HaveCount(16);
    }

    [Test]
    public void Should_ReturnNineTiles_When_CanvasIsThreeByThree_AndAlignedCoordinate()
    {
        var alignedTile = new TileCoordinate(512, 512, 10);
        var (lat, lng) = WebMercatorProjection.ToLatLngCenter(alignedTile);

        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(lat, lng), 10, MediumCanvas, MediumCanvas);
        result.Should().HaveCount(9);
    }

    [Test]
    public void GetVisibleTiles_CenterTile_HasCorrectPixelPosition()
    {
        // Center tile pixel position should be at canvas center
        // offset by the center coordinate's position within the tile
        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(0, 0),
            zoom: 3,
            canvasWidth: SmallCanvas,
            canvasHeight: SmallCanvas);

        var centerTile = result.Single(t =>
            t.Coordinate == WebMercatorProjection.FromLatLng(0, 0, 3));

        centerTile.PixelPosition.X.Should().BeApproximately(SmallCanvas / 2f, 1f);
        centerTile.PixelPosition.Y.Should().BeApproximately(SmallCanvas / 2f, 1f);
    }

    [Test]
    public void GetVisibleTiles_NullIsland_ZoomOne_ReturnsFourTiles()
    {
        // Null Island (0,0) at zoom 1 sits at the intersection
        // of all 4 tiles so all 4 should be visible
        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(0, 0),
            zoom: 3,
            canvasWidth: SmallCanvas,
            canvasHeight: SmallCanvas);

        result.Should().HaveCount(4);
        result.Should().Contain(t => t.Coordinate == new TileCoordinate(3, 3, 3));
        result.Should().Contain(t => t.Coordinate == new TileCoordinate(4, 3, 3));
        result.Should().Contain(t => t.Coordinate == new TileCoordinate(3, 4, 3));
        result.Should().Contain(t => t.Coordinate == new TileCoordinate(4, 4, 3));
    }

    [Test]
    public void GetVisibleTiles_InvalidZoom_ThrowsArgumentOutOfRangeException()
    {
        var act = () => ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(0, 0),
            zoom: WebMercatorProjection.MaxZoom + 1,
            canvasWidth: SmallCanvas,
            canvasHeight: SmallCanvas);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetVisibleTiles_NoDuplicateTiles()
    {
        var result = ViewportCalculator.GetVisibleTiles(
            new GeoCoordinate(40.7128, -74.0060),
            zoom: 12,
            canvasWidth: MediumCanvas,
            canvasHeight: MediumCanvas);

        result.Select(t => t.Coordinate)
            .Should().OnlyHaveUniqueItems();
    }
}