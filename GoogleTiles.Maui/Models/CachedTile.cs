using SkiaSharp;

namespace GoogleTiles.Maui.Models;

internal record CachedTile(SKBitmap Bitmap, DateTimeOffset? ExpiresAt)
{
    public DateTimeOffset LastAccessed { get; private set; } = DateTime.UtcNow;
    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow >= ExpiresAt;

    public void Touch() => LastAccessed = DateTimeOffset.UtcNow;
}