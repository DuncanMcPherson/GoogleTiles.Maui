using GoogleTiles.Maui.Core.Abstractions;

namespace GoogleTiles.Maui.Core.Infrastructure;

internal class SystemTimeProvider : ITimeProvider
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}