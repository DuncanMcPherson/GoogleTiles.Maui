using SkiaSharp.Views.iOS;
using UIKit;

namespace GoogleTiles.Maui.Gestures;

internal partial class RotationGestureHandler
{
    private UIRotationGestureRecognizer? _recognizer;
    private nfloat _lastRotation;

    internal void Attach(SKMetalView view)
    {
        _recognizer = new UIRotationGestureRecognizer(HandleRotation)
        {
            Delegate = new AllowSimultaneousGestureDelegate()
        };
        view.AddGestureRecognizer(_recognizer);
    }

    internal void Detach(SKMetalView view)
    {
        if (_recognizer is null) return;
        view.RemoveGestureRecognizer(_recognizer);
        _recognizer.Dispose();
        _recognizer = null;
    }
    private void HandleRotation(UIRotationGestureRecognizer recognizer)
    {
        switch (recognizer.State)
        {
            case UIGestureRecognizerState.Began:
                IsTwoFingerGesture = true;
                _lastRotation = recognizer.Rotation;
                break;

            case UIGestureRecognizerState.Changed:
                var rotationDelta = recognizer.Rotation - _lastRotation;
                var degrees = (float)(rotationDelta * 180 / Math.PI);
                OnRotationDeltaChanged(degrees);
                _lastRotation = recognizer.Rotation;
                break;

            case UIGestureRecognizerState.Ended:
            case UIGestureRecognizerState.Cancelled:
            case UIGestureRecognizerState.Failed:
            default:
                IsTwoFingerGesture = false;
                ResetRotationDelta();
                break;
        }
    }

    private class AllowSimultaneousGestureDelegate : UIGestureRecognizerDelegate
    {
        public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer,
            UIGestureRecognizer otherGestureRecognizer) => true;
    }
}