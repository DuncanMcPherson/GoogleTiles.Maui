namespace GoogleTiles.Maui.Core.Tests;

[TestFixture]
public class SessionTokenTests
{
    [Test]
    public void IsExpired_ExpiryInFuture_ReturnsFalse()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(1));
        token.IsExpired.Should().BeFalse();
    }

    [Test]
    public void IsExpired_ExpiryInPast_ReturnsTrue()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow.AddHours(-1));
        token.IsExpired.Should().BeTrue();
    }

    [Test]
    public void IsExpired_ExpiryIsNow_ReturnsTrue()
    {
        var token = new SessionToken("test-token", DateTimeOffset.UtcNow);
        token.IsExpired.Should().BeTrue();
    }
}