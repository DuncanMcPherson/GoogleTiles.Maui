namespace GoogleTiles.Maui.Core.Abstractions;

internal interface ITimeProvider
{
    /// <summary>
    /// Gets the current date and time in Coordinated Universal Time (UTC).
    /// </summary>
    DateTimeOffset UtcNow { get; }
}