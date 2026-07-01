using System.Collections.Concurrent;
using System.Diagnostics;
using SkiaSharp;

namespace GoogleTiles.Maui.Resolvers;

internal partial class PinImageResolver
{
    private readonly IImageSourceServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<ImageSource, SKBitmap> _cache = new();

    public PinImageResolver(IImageSourceServiceProvider serviceProvider, IHttpClientFactory httpsClientFactory)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpsClientFactory;
    }

    public async Task<SKBitmap?> ResolveAsync(
        ImageSource? source,
        CancellationToken ct = default)
    {
        if (source is null)
            return null;

        if (_cache.TryGetValue(source, out var cached))
            return cached;

        try
        {
            var bitmap = await ResolvePlatformAsync(source, ct);
            if (bitmap is not null)
                _cache[source] = bitmap;
            return bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Pin image resolution failed: {ex.Message}");
            return null;
        }
    }
}