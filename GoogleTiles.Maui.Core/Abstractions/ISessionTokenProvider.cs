using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Abstractions;

internal interface ISessionTokenProvider
{
    /// <summary>
    /// Gets a session token for accessing Google Maps services. The token should be valid for a reasonable duration and should be refreshed as needed.
    /// </summary>
    /// <param name="cancellation">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the session token.</returns>
    Task<SessionToken> GetTokenAsync(CancellationToken cancellation = default);
}