using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Infrastructure;
using GoogleTiles.Maui.Core.Models;
using GoogleTiles.Maui.Core.Session;
using GoogleTiles.Maui.Core.Tiles;
using GoogleTiles.Maui.Handlers;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace GoogleTiles.Maui;

// All the code in this file is included in all platforms.
public static class MauiAppBuilderExtensions
{

    public static MauiAppBuilder UseGoogleTiles(
        this MauiAppBuilder builder,
        Action<GoogleTilesOptions> configure)
    {
        // TODO: add skia sharp
        builder.UseSkiaSharp();

        var options = new GoogleTilesOptions();
        configure(options);

        ValidateOptions(options);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        builder.Services.AddSingleton<SessionTokenCache>();
        builder.Services.AddSingleton<ISessionTokenProvider, SessionTokenProvider>();
        builder.Services.AddSingleton<TileFetcher>();
        builder.Services.AddSingleton<ViewportMetadataFetcher>();
        builder.Services.AddHttpClient("GoogleTiles");

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<GoogleTilesView, GoogleTilesViewHandler>();
        });

        return builder;
    }

    private static void ValidateOptions(GoogleTilesOptions options)
    {
        if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrWhiteSpace(options.ApiKey))
            throw new InvalidOperationException(
                "A Google Maps API key must be provided via options.ApiKey. " +
                "See https://developers.google.com/maps/documentation/tile/get-api-key for details.");
    }
}