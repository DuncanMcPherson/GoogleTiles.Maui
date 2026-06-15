namespace GoogleTiles.Maui.Core.Models;

public record SessionToken(string Token, DateTimeOffset Expiry)
{
    public bool IsExpired => DateTimeOffset.UtcNow >= Expiry;
}