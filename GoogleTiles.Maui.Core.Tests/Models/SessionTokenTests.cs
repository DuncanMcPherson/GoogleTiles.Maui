namespace GoogleTiles.Maui.Core.Tests.Models;

[TestFixture]
public class SessionTokenTests
{
    private ITimeProvider _timeProvider;

    [SetUp]
    public void SetUp()
    {
        _timeProvider = Substitute.For<ITimeProvider>();
        _timeProvider.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    [Test]
    public void IsExpired_ExpiryInFuture_ReturnsFalse()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        token.IsExpired(_timeProvider).Should().BeFalse();
    }

    [Test]
    public void IsExpired_ExpiryInPast_ReturnsTrue()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(-1));
        token.IsExpired(_timeProvider).Should().BeTrue();
    }

    [Test]
    public void IsExpired_ExpiryIsNow_ReturnsTrue()
    {
        var token = new SessionToken("test-token", _timeProvider.UtcNow);
        token.IsExpired(_timeProvider).Should().BeTrue();
    }
}