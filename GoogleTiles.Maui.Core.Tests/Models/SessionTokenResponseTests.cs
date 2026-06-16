namespace GoogleTiles.Maui.Core.Tests.Models;

[TestFixture]
public class SessionTokenResponseTests
{
    [Test]
    public void ToSessionToken_ValidResponse_ReturnsCorrectToken()
    {
        var expiry = DateTimeOffset.UtcNow.AddHours(1);
        var response = new SessionTokenResponse
        {
            Session = "test-token",
            Expiry = expiry.ToUnixTimeSeconds().ToString()
        };

        var token = response.ToSessionToken();

        token.Should().BeOfType<SessionToken>();
        token.Token.Should().Be("test-token");
        token.Expiry.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void ToSessionToken_InvalidExpiry_ThrowsFormatException()
    {
        var response = new SessionTokenResponse
        {
            Session = "test-token",
            Expiry = "not-a-timestamp"
        };

        Action act = () => response.ToSessionToken();
        act.Should().Throw<FormatException>();
    }
}