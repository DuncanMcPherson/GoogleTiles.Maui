using SkiaSharp;

namespace GoogleTiles.Maui.Resolvers;

internal partial class PinImageResolver
{
    private async Task<SKBitmap?> ResolvePlatformAsync(ImageSource source,
        CancellationToken ct)
    {
        var service = _serviceProvider.GetImageSourceService(source);
        if (service is null)
            return null;

        var result = await service.GetImageAsync(source, cancellationToken: ct);
        if (result?.Value is not { } uiImage)
            return null;

        using var cgImage = uiImage.CGImage;
        if (cgImage is null) return null;

        using var stream = new MemoryStream();
        using var nsData = uiImage.AsPNG();
        if (nsData is null)
            return null;

        await nsData.AsStream().CopyToAsync(stream, ct);
        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }
}