using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;

namespace GoogleTiles.Maui.Gestures;

internal class PanGestureHandler
{
    public event Action<float, float>? PanDeltaChanged;

    internal void Attach(SKSwapChainPanel view)
    {
        view.ManipulationMode |= ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        view.ManipulationDelta += OnManipulationDelta;
    }

    internal void Detach(SKSwapChainPanel view) => view.ManipulationDelta -= OnManipulationDelta;

    private void OnManipulationDelta(object? sender, ManipulationDeltaRoutedEventArgs e)
    {
        var translation = e.Delta.Translation;
        if (translation.X != 0 || translation.Y != 0)
        {
            PanDeltaChanged?.Invoke((float)translation.X, (float)translation.Y);
        }
    }
}