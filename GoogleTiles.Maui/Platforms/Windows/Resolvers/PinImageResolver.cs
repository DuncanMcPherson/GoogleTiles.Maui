using System.Diagnostics;
using SkiaSharp;

namespace GoogleTiles.Maui.Resolvers;

internal partial class PinImageResolver
{
    private async Task<SKBitmap?> ResolvePlatformAsync(ImageSource source, CancellationToken ct)
    {
        try
        {
            if (source is UriImageSource uriSource)
            {
                using var client = _httpClientFactory.CreateClient("GoogleTiles");
                var bytes = await client.GetByteArrayAsync(uriSource.Uri, ct);
                return SKBitmap.Decode(bytes);
            }

            if (source is FileImageSource fileSource)
            {
                var bytes = await File.ReadAllBytesAsync(fileSource.File, ct);
                return SKBitmap.Decode(bytes);
            }

            if (source is StreamImageSource streamSource)
            {
                var stream = await streamSource.Stream(ct);
                if (stream is null)
                    return null;
                return SKBitmap.Decode(stream);
            }

            var service = _serviceProvider.GetImageSourceService(source);
            if (service is null)
                return null;

            var result = await service.GetImageSourceAsync(source, cancellationToken: ct);
            if (result?.Value is Microsoft.UI.Xaml.Media.Imaging.BitmapImage { UriSource: not null } bitmapImage)
            {
                var file = await Windows.Storage.StorageFile
                    .GetFileFromApplicationUriAsync(bitmapImage.UriSource);
                await using var stream = await file.OpenStreamForReadAsync();
                return SKBitmap.Decode(stream);
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows pin image resolution failed: {ex.Message}");
            return null;
        }
    }
}