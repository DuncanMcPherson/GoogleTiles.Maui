using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;

namespace GoogleTiles.Maui.Gestures;

public class ScaleGestureHandler
{
    public event Action<float>? ZoomDeltaChanged;

    internal void Attach(SKSwapChainPanel view)
    {
        view.ManipulationMode |= ManipulationModes.Scale;
        view.ManipulationDelta += OnManipulationDelta;
    }

    internal void Detach(SKSwapChainPanel view)
    {
        view.ManipulationDelta -= OnManipulationDelta;
    }

    private void OnManipulationDelta(object? sender, ManipulationDeltaRoutedEventArgs e)
    {
        var scale = e.Delta.Scale;
        if (Math.Abs(scale - 1f) > 0.0001)
            ZoomDeltaChanged?.Invoke(scale);
    }
}