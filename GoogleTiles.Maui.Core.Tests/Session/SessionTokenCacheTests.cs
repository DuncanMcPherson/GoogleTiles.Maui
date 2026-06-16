namespace GoogleTiles.Maui.Core.Tests.Session;

[TestFixture]
public class SessionTokenCacheTests
{
    private ITimeProvider _timeProvider;
    private SessionTokenCache _cache;

    [SetUp]
    public void SetUp()
    {
        _timeProvider = Substitute.For<ITimeProvider>();
        _timeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
        _cache = new SessionTokenCache(_timeProvider);
    }

    [Test]
    public void HasValitToken_NoTokenCached_ReturnsFalse()
    {
        _cache.HasValidToken.Should().BeFalse();
    }

    [Test]
    public void HasValidToken_ValidTokenCached_ReturnsTrue()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        _cache.Store(token);
        _cache.HasValidToken.Should().BeTrue();
    }

    [Test]
    public void HasValidToken_ExpiredTokenCached_ReturnsFalse()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(-1));
        _cache.Store(token);
        _cache.HasValidToken.Should().BeFalse();
    }

    [Test]
    public void Current_NoTokenCached_ReturnsNull()
    {
        _cache.Current.Should().BeNull();
    }

    [Test]
    public void Current_ValidTokenCached_ReturnsToken()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        _cache.Store(token);
        _cache.Current.Should().Be(token);
    }

    [Test]
    public void Current_ExpiredTokenCached_ReturnsNull()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(-1));
        _cache.Store(token);
        _cache.Current.Should().BeNull();
    }

    [Test]
    public void Store_ReplacesExistingToken()
    {
        var first = new SessionToken("first-token", DateTimeOffset.UtcNow.AddHours(1));
        var second = new SessionToken("second-token", DateTimeOffset.UtcNow.AddHours(2));

        _cache.Store(first);
        _cache.Store(second);

        _cache.Current.Should().Be(second);
    }
}