using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;

namespace GoogleTiles.Maui.Gestures;

internal partial class RotationGestureHandler
{
    internal void Attach(SKSwapChainPanel view)
    {
        if (view.ManipulationMode is ManipulationModes.None or ManipulationModes.System or ManipulationModes.All)
            view.ManipulationMode = ManipulationModes.Rotate;
        else
            view.ManipulationMode |= ManipulationModes.Rotate;

        view.ManipulationStarted += OnManipulationStarted;
        view.ManipulationDelta += OnManipulationDelta;
        view.ManipulationCompleted += OnManipulationCompleted;
    }

    internal void Detach(SKSwapChainPanel view)
    {
        
        view.ManipulationStarted -= OnManipulationStarted;
        view.ManipulationDelta -= OnManipulationDelta;
        view.ManipulationCompleted -= OnManipulationCompleted;
    }

    private void OnManipulationStarted(object? sender, ManipulationStartedRoutedEventArgs e)
    {
        IsTwoFingerGesture = true;
    }

    private void OnManipulationDelta(object? sender, ManipulationDeltaRoutedEventArgs e)
    {
        var degrees = e.Delta.Rotation;
        if (degrees != 0f)
        {
            OnRotationDeltaChanged(degrees);
        }
    }

    private void OnManipulationCompleted(object? sender, ManipulationCompletedRoutedEventArgs e)
    {
        IsTwoFingerGesture = false;
        ResetRotationDelta();
    }
}