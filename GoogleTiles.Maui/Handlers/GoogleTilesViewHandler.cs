using GoogleTiles.Maui.Gestures;
using SkiaSharp.Views.Maui.Handlers;

namespace GoogleTiles.Maui.Handlers;

public partial class GoogleTilesViewHandler : SKGLViewHandler
{
    private RotationGestureHandler? _rotationGestureHandler;
}