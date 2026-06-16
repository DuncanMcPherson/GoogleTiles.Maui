using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Tiles;

internal class TileFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ISessionTokenProvider _sessionTokenProvider;
    private readonly GoogleTilesOptions _options;

    public TileFetcher(
        IHttpClientFactory factory,
        ISessionTokenProvider sessionTokenProvider,
        GoogleTilesOptions options)
    {
        _httpClient = factory.CreateClient("GoogleTiles");
        _sessionTokenProvider = sessionTokenProvider;
        _options = options;
    }

    public async Task<TileData> FetchAsync(TileCoordinate coordinate, CancellationToken cancellationToken = default)
    {
        var token = await _sessionTokenProvider.GetTokenAsync(cancellationToken);
        var url = BuildTileUrl(coordinate, token);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        var cacheControl = response.Headers.CacheControl?.ToString();
        var cachePolicy = CacheControlParser.Parse(cacheControl);

        return cachePolicy with { Bytes = bytes, ContentType = contentType };
    }

    private string BuildTileUrl(TileCoordinate coordinate, SessionToken token) =>
        $"https://tile.googleapis.com/v1/2dtiles/{coordinate.Zoom}/{coordinate.X}/{coordinate.Y}" +
        $"?session={token.Token}&key={_options.ApiKey}";
}