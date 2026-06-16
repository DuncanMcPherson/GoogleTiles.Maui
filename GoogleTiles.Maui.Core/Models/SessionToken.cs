using GoogleTiles.Maui.Core.Abstractions;

namespace GoogleTiles.Maui.Core.Models;

/// <summary>
/// Represents a session token with its expiry time. This token is used for authenticating requests to the Google Tiles API.
/// </summary>
/// <param name="Token">The token string used for authentication.</param>
/// <param name="Expiry">The expiration time of the token.</param>
internal record SessionToken(string Token, DateTimeOffset Expiry)
{
    /// <summary>
    /// Determines whether the session token has expired based on the current time provided by the ITimeProvider.
    /// </summary>
    /// <param name="timeProvider">The time provider used to get the current time.</param>
    /// <returns>True if the token has expired; otherwise, false.</returns>
    public bool IsExpired(ITimeProvider timeProvider) => timeProvider.UtcNow >= Expiry;
}