namespace GoogleTiles.Maui.Core.Models;

/// <summary>
/// Represents the response from the session token API, containing the session token, its expiry time, and tile information.
/// </summary>
internal class SessionTokenResponse
{
    /// <summary>
    /// The session token string that can be used for authenticated requests to the tile service. This token is typically a long, opaque string that serves as a credential for accessing protected resources. It should be treated securely and not exposed in client-side code or logs.
    /// </summary>
    [JsonPropertyName("session")] public string Session { get; init; } = string.Empty;
    /// <summary>
    /// The expiry time of the session token, represented as a Unix timestamp in seconds. This indicates when the token will become invalid and should no longer be used for authentication. Clients should check this value to determine when to request a new token before making authenticated requests to the tile service.
    /// </summary>
    [JsonPropertyName("expiry")] public string Expiry { get; init; } = string.Empty;
    /// <summary>
    /// The width of the tiles in pixels. This value indicates the horizontal dimension of each tile that will be served by the tile service. It is important for clients to know this value to correctly request and display tiles, ensuring that they are rendered at the appropriate size and resolution.
    /// </summary>
    [JsonPropertyName("tileWidth")] public int TileWidth { get; init; }
    /// <summary>
    /// The height of the tiles in pixels. This value indicates the vertical dimension of each tile that will be served by the tile service. It is important for clients to know this value to correctly request and display tiles, ensuring that they are rendered at the appropriate size and resolution.
    /// </summary>
    [JsonPropertyName("tileHeight")] public int TileHeight { get; init; }
    [JsonPropertyName("imageFormat")] public string ImageFormat { get; init; } = string.Empty;

    public SessionToken ToSessionToken()
    {
        if (!long.TryParse(Expiry, out var unixSeconds))
            throw new FormatException($"Invalid expiry timestamp: {Expiry}");

        var expiry = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        return new SessionToken(Session, expiry);
    }
}