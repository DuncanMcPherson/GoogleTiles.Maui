using Microsoft.Extensions.Logging;

namespace GoogleTiles.Maui.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseGoogleTiles(options =>
            {
                options.ApiKey = "AIzaSyDXXDWct7DRK1VBGnBZIgqodyqBsu7d1cg";
                options.EnableCaching = true;
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}