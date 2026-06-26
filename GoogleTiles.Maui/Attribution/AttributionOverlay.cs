using SkiaSharp;

namespace GoogleTiles.Maui.Attribution;

internal class AttributionOverlay
{
    private const string FallbackText = "Google Maps";
    private const float LogoHeight = 18f;
    private const float Padding = 8f;
    private const float TextSize = 11f;
    private static readonly SKColor TextColor = SKColor.Parse("#5e5e5e");
    private static readonly SKColor BackgroundColor = new(255, 255, 255, 180);

    private SKBitmap? _logoBitmap;
    private bool _logoLoaded;

    public void LoadLogo(byte[]? logoBytes)
    {
        if (logoBytes is null) return;
        _logoBitmap = SKBitmap.Decode(logoBytes);
        _logoLoaded = _logoBitmap is not null;
    }

    public void Draw(SKCanvas canvas, int canvasWidth, int canvasHeight, string copyrightText)
    {
        DrawLogo(canvas, canvasHeight);
        DrawCopyright(canvas, canvasWidth, canvasHeight, copyrightText);
    }

    private void DrawLogo(SKCanvas canvas, int canvasHeight)
    {
        if (_logoLoaded && _logoBitmap is not null)
        {
            var aspectRatio = (float)_logoBitmap.Width / _logoBitmap.Height;
            var logoWidth = LogoHeight * aspectRatio;
            var destRect = new SKRect(
                Padding,
                canvasHeight - Padding - LogoHeight,
                Padding + logoWidth,
                canvasHeight - Padding);

            canvas.DrawBitmap(_logoBitmap, destRect);
        }
        else
        {
            using var font = new SKFont(SKTypeface.FromFamilyName("sans-serif"), TextSize + 2);
            using var paint = new SKPaint();
            paint.Color = TextColor;
            paint.IsAntialias = true;

            canvas.DrawText(FallbackText, Padding, canvasHeight - Padding, font, paint);
        }
    }

    private void DrawCopyright(SKCanvas canvas, int canvasWidth, int canvasHeight, string copyrightText)
    {
        using var font = new SKFont(SKTypeface.FromFamilyName("sans-serif"), TextSize);
        using var textColor = new SKPaint();
        textColor.Color = TextColor;
        var textWidth = font.MeasureText(copyrightText);

        using var bgPaint = new SKPaint();
        bgPaint.Color = BackgroundColor;
        var bgRect = new SKRect(
            canvasWidth - textWidth - Padding * 2,
            canvasHeight - TextSize - Padding * 2,
            canvasWidth,
            canvasHeight);

        canvas.DrawRect(bgRect, bgPaint);
        canvas.DrawText(copyrightText, canvasWidth - textWidth - Padding,
            canvasHeight - Padding, font, textColor);
    }
}