using System.Diagnostics;
using System.Text.Json;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Tiles;

internal class ViewportMetadataFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionTokenProvider _sessionTokenProvider;
    private readonly GoogleTilesOptions _options;

    public ViewportMetadataFetcher(
        IHttpClientFactory httpClientFactory,
        ISessionTokenProvider sessionTokenProvider,
        GoogleTilesOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _sessionTokenProvider = sessionTokenProvider;
        _options = options;
    }

    public async Task<ViewportMetadata?> FetchAsync(
        int zoom,
        double north,
        double south,
        double east,
        double west,
        CancellationToken ct = default)
    {
        try
        {
            var token = await _sessionTokenProvider.GetTokenAsync(ct);
            var client = _httpClientFactory.CreateClient("GoogleTiles");

            var url = $"https://tile.googleapis.com/tile/v1/viewport" +
                      $"?session={token.Token}" +
                      $"&key={_options.ApiKey}" +
                      $"&zoom={zoom}" +
                      $"&north={north}" +
                      $"&south={south}" +
                      $"&east={east}" +
                      $"&west={west}";
            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ViewportMetadata>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Viewport metadata fetch failed: {ex.Message}");
            return null;
        }
    }
}