using SkiaSharp;
using Microsoft.Extensions.Caching.Memory;

namespace Flygio.Services;

public class OgImageService
{
    private readonly IMemoryCache _cache;

    public OgImageService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public byte[] GenerateRouteImage(string origin, string destination, string originCode, string destCode, decimal? price)
    {
        var cacheKey = $"og-route-{originCode}-{destCode}-{price:F0}";
        if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
            return cached;

        var bytes = RenderRouteImage(origin, destination, originCode, destCode, price);
        _cache.Set(cacheKey, bytes, TimeSpan.FromHours(6));
        return bytes;
    }

    public byte[] GenerateDestinationImage(string city, string country, string airportCode, decimal? price)
    {
        var cacheKey = $"og-dest-{airportCode}-{price:F0}";
        if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
            return cached;

        var bytes = RenderDestinationImage(city, country, airportCode, price);
        _cache.Set(cacheKey, bytes, TimeSpan.FromHours(6));
        return bytes;
    }

    public byte[] GenerateDefaultImage()
    {
        var cacheKey = "og-default";
        if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
            return cached;

        var bytes = RenderDefaultImage();
        _cache.Set(cacheKey, bytes, TimeSpan.FromHours(24));
        return bytes;
    }

    private static byte[] RenderRouteImage(string origin, string destination, string originCode, string destCode, decimal? price)
    {
        const int width = 1200;
        const int height = 630;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        DrawGradientBackground(canvas, width, height);
        DrawLogo(canvas, width);

        // Route text: "Stockholm -> Bangkok"
        using var routeFont = new SKFont(SKTypeface.Default, 52) { Edging = SKFontEdging.Antialias };
        using var routePaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        var routeText = $"{origin}  \u2192  {destination}";
        canvas.DrawText(routeText, 80, 280, routeFont, routePaint);

        // Airport codes
        using var codeFont = new SKFont(SKTypeface.Default, 28) { Edging = SKFontEdging.Antialias };
        using var codePaint = new SKPaint { Color = new SKColor(180, 200, 255), IsAntialias = true };
        canvas.DrawText($"{originCode}  \u2192  {destCode}", 80, 330, codeFont, codePaint);

        // Price if available
        if (price.HasValue)
        {
            using var priceFont = new SKFont(SKTypeface.Default, 64) { Edging = SKFontEdging.Antialias };
            using var pricePaint = new SKPaint { Color = new SKColor(134, 239, 172), IsAntialias = true };
            canvas.DrawText($"fr\u00e5n {price.Value:N0} kr", 80, 430, priceFont, pricePaint);
        }

        // Tagline
        using var tagFont = new SKFont(SKTypeface.Default, 22) { Edging = SKFontEdging.Antialias };
        using var tagPaint = new SKPaint { Color = new SKColor(160, 180, 220), IsAntialias = true };
        canvas.DrawText("J\u00e4mf\u00f6r flygpriser \u2022 Hitta billiga flyg", 80, 540, tagFont, tagPaint);

        return EncodeToPng(surface);
    }

    private static byte[] RenderDestinationImage(string city, string country, string airportCode, decimal? price)
    {
        const int width = 1200;
        const int height = 630;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        DrawGradientBackground(canvas, width, height);
        DrawLogo(canvas, width);

        // Destination text
        using var cityFont = new SKFont(SKTypeface.Default, 56) { Edging = SKFontEdging.Antialias };
        using var cityPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText($"Flyg till {city}", 80, 280, cityFont, cityPaint);

        // Country and airport code
        using var subFont = new SKFont(SKTypeface.Default, 28) { Edging = SKFontEdging.Antialias };
        using var subPaint = new SKPaint { Color = new SKColor(180, 200, 255), IsAntialias = true };
        canvas.DrawText($"{country} \u2022 {airportCode}", 80, 330, subFont, subPaint);

        if (price.HasValue)
        {
            using var priceFont = new SKFont(SKTypeface.Default, 64) { Edging = SKFontEdging.Antialias };
            using var pricePaint = new SKPaint { Color = new SKColor(134, 239, 172), IsAntialias = true };
            canvas.DrawText($"fr\u00e5n {price.Value:N0} kr", 80, 430, priceFont, pricePaint);
        }

        using var tagFont = new SKFont(SKTypeface.Default, 22) { Edging = SKFontEdging.Antialias };
        using var tagPaint = new SKPaint { Color = new SKColor(160, 180, 220), IsAntialias = true };
        canvas.DrawText("J\u00e4mf\u00f6r flygpriser fr\u00e5n Sverige", 80, 540, tagFont, tagPaint);

        return EncodeToPng(surface);
    }

    private static byte[] RenderDefaultImage()
    {
        const int width = 1200;
        const int height = 630;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        DrawGradientBackground(canvas, width, height);
        DrawLogo(canvas, width);

        using var titleFont = new SKFont(SKTypeface.Default, 48) { Edging = SKFontEdging.Antialias };
        using var titlePaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText("Hitta billiga flyg fr\u00e5n Sverige", 80, 300, titleFont, titlePaint);

        using var subFont = new SKFont(SKTypeface.Default, 28) { Edging = SKFontEdging.Antialias };
        using var subPaint = new SKPaint { Color = new SKColor(180, 200, 255), IsAntialias = true };
        canvas.DrawText("J\u00e4mf\u00f6r priser \u2022 Prishistorik \u2022 Prisvarningar", 80, 360, subFont, subPaint);

        using var tagFont = new SKFont(SKTypeface.Default, 22) { Edging = SKFontEdging.Antialias };
        using var tagPaint = new SKPaint { Color = new SKColor(160, 180, 220), IsAntialias = true };
        canvas.DrawText("flygio.se", 80, 540, tagFont, tagPaint);

        return EncodeToPng(surface);
    }

    private static void DrawGradientBackground(SKCanvas canvas, int width, int height)
    {
        using var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [new SKColor(30, 58, 138), new SKColor(79, 70, 229)],
                SKShaderTileMode.Clamp),
            IsAntialias = true
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        // Subtle pattern overlay
        using var overlayPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 10),
            IsAntialias = true
        };
        for (int i = 0; i < 5; i++)
        {
            canvas.DrawCircle(width - 200 + i * 80, 150 + i * 60, 120 - i * 15, overlayPaint);
        }
    }

    private static void DrawLogo(SKCanvas canvas, int width)
    {
        using var logoFont = new SKFont(SKTypeface.Default, 36) { Edging = SKFontEdging.Antialias };
        using var logoPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText("Flygio", 80, 120, logoFont, logoPaint);

        // Accent line under logo
        using var linePaint = new SKPaint
        {
            Color = new SKColor(96, 165, 250),
            StrokeWidth = 3,
            IsAntialias = true
        };
        canvas.DrawLine(80, 135, 200, 135, linePaint);
    }

    private static byte[] EncodeToPng(SKSurface surface)
    {
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }
}
