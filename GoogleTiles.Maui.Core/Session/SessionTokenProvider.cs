using System.Text;
using System.Text.Json;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Extensions;
using GoogleTiles.Maui.Core.Models;

namespace GoogleTiles.Maui.Core.Session;

internal class SessionTokenProvider : ISessionTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly SessionTokenCache _cache;
    private readonly GoogleTilesOptions _options;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SessionTokenProvider(
        IHttpClientFactory factory,
        SessionTokenCache cache,
        GoogleTilesOptions options)
    {
        _httpClient = factory.CreateClient("GoogleTiles");
        _cache = cache;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<SessionToken> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.HasValidToken)
            return _cache.Current!;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_cache.HasValidToken)
                return _cache.Current!;

            var token = await FetchTokenAsync(cancellationToken);
            _cache.Store(token);
            return token;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<SessionToken> FetchTokenAsync(CancellationToken cancellationToken)
    {
        var styles = _options.Theme switch
        {
            MapTheme.Night when _options.MapType == MapType.Roadmap => LoadNightStyle(),
            MapTheme.Custom => _options.CustomThemeJson,
            _ => null
        };
        var url = $"https://tile.googleapis.com/v1/createSession?key={_options.ApiKey}";
        var body = new
        {
            mapType = _options.MapType.ToApiString(),
            language = _options.Language,
            region = _options.Region,
            imageFormat = _options.ImageFormat.ToApiString(),
            scale = _options.Scale.ToApiString(),
            highDpi = _options.HighDpi,
            styles = styles is not null
                ? JsonSerializer.Deserialize<JsonElement>(styles)
                : (JsonElement?)null,
        };
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<SessionTokenResponse>(responseJson) ??
                            throw new InvalidOperationException("Failed to deserialize session token response");
        return tokenResponse.ToSessionToken();
    }

    private static string LoadNightStyle()
    {
        var assembly = typeof(SessionTokenProvider).Assembly;
        using var stream = assembly.GetManifestResourceStream("GoogleTiles.Maui.Core.Resources.dark_theme.json");

        if (stream is null)
            throw new InvalidOperationException("Dark theme resource not found");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}