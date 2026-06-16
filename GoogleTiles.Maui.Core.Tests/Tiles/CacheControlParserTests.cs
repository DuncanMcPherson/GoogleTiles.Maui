namespace GoogleTiles.Maui.Core.Tests.Tiles;

[TestFixture]
public class CacheControlParserTests
{
    [Test]
    public void Should_ReturnCorrectDuration_When_MaxAgePresent()
    {
        var result = CacheControlParser.Parse("max-age=3600");
        result.MaxAge.Should().Be(TimeSpan.FromSeconds(3600));
        result.NoStore.Should().BeFalse();
    }

    [Test]
    public void Should_ReturnNoStore_When_NoStorePresent()
    {
        var result = CacheControlParser.Parse("no-store");
        result.MaxAge.Should().BeNull();
        result.NoStore.Should().BeTrue();
    }

    [Test]
    public void Should_ReturnNullMaxAge_When_NoCache()
    {
        var result = CacheControlParser.Parse("no-cache");
        result.MaxAge.Should().BeNull();
        result.NoStore.Should().BeFalse();
    }

    [Test]
    public void Should_ReturnNullMaxAge_When_NullHeader()
    {
        var result = CacheControlParser.Parse(null);
        result.MaxAge.Should().BeNull();
        result.NoStore.Should().BeTrue();
    }

    [Test]
    public void Parse_EmptyHeader_ReturnsNullMaxAge()
    {
        var result = CacheControlParser.Parse(string.Empty);
        result.MaxAge.Should().BeNull();
        result.NoStore.Should().BeTrue();
    }

    [Test]
    public void Parse_MaxAgeWithNoStore_NoStoreTakesPrecedence()
    {
        var result = CacheControlParser.Parse("max-age=3600, no-store");
        result.NoStore.Should().BeTrue();
        result.MaxAge.Should().BeNull();
    }

    [Test]
    public void Parse_MaxAgeWithNoCache_ReturnsNullMaxAge()
    {
        var result = CacheControlParser.Parse("max-age=3600, no-cache");
        result.MaxAge.Should().BeNull();
        result.NoStore.Should().BeFalse();
    }
}