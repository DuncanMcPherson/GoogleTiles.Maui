using SkiaSharp;

namespace GoogleTiles.Maui.Resolvers;

internal partial class PinImageResolver
{
    private async Task<SKBitmap?> ResolvePlatformAsync(ImageSource source, CancellationToken ct)
    {
        var service = _serviceProvider.GetImageSourceService(source);
        if (service is null)
            return null;

        var drawable = await service.GetDrawableAsync(source, Android.App.Application.Context, ct);

        if (drawable?.Value is not Android.Graphics.Drawables.BitmapDrawable bitmapDrawable)
            return null;

        var androidBitmap = bitmapDrawable.Bitmap;
        if (androidBitmap is null)
            return null;

        using var stream = new MemoryStream();
        await androidBitmap.CompressAsync(
            Android.Graphics.Bitmap.CompressFormat.Png!,
            100,
            stream);

        stream.Position = 0;
        return SKBitmap.Decode(stream);
    }
}