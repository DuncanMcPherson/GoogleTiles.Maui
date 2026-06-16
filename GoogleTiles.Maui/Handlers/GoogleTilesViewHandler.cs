using GoogleTiles.Maui.Controls;
using GoogleTiles.Maui.Core.Abstractions;
using GoogleTiles.Maui.Core.Tiles;
using SkiaSharp.Views.Maui.Handlers;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler : SKCanvasViewHandler
{
    private TileFetcher? _tileFetcher;
    private ISessionTokenProvider? _sessionTokenProvider;

    public static PropertyMapper<GoogleTilesView, GoogleTilesViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(GoogleTilesView.Center)] = MapCenter,
        [nameof(GoogleTilesView.ZoomLevel)] = MapZoomLevel
    };

    public GoogleTilesViewHandler() : base(Mapper, null)
    {
    }

    static partial void MapCenter(GoogleTilesViewHandler handler, GoogleTilesView view);

    static partial void MapZoomLevel(GoogleTilesViewHandler handler, GoogleTilesView view);
}