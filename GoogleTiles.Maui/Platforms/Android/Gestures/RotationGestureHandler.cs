using Android.Views;
using SkiaSharp.Views.Android;
using View = Android.Views.View;

namespace GoogleTiles.Maui.Gestures;

internal partial class RotationGestureHandler
{
    private float _lastAngle;
    private float _lastDistance;
    private bool _isRotating;

    private const float RotationThresholdDegrees = 3f;
    private const float ZoomDominanceRatio = 2.5f;

    internal void Attach(GLTextureView platformView) => platformView.Touch += OnTouch;
    internal void Detach(GLTextureView platformView) => platformView.Touch -= OnTouch;

    private void OnTouch(object? sender, View.TouchEventArgs e)
    {
        var motionEvent = e.Event;
        if (motionEvent is not null && motionEvent.PointerCount > 1)
        {
            var angle = GetAngle(motionEvent);
            var distance = GetDistance(motionEvent);

            switch (motionEvent.ActionMasked)
            {
                case MotionEventActions.PointerDown:
                    IsTwoFingerGesture = true;
                    _lastAngle = angle;
                    _lastDistance = distance;
                    _isRotating = true;
                    break;
                case MotionEventActions.Move:
                    if (!_isRotating) break;
                    var delta = angle - _lastAngle;
                    if (delta > 180f) delta -= 360f;
                    if (delta < -180f) delta += 360f;

                    var distanceDelta = Math.Abs(distance - _lastDistance);
                    var normalizedDistanceDelta = _lastDistance > 0
                        ? distanceDelta / _lastDistance * 100f
                        : 0f;

                    var isLikelyZoom = normalizedDistanceDelta > Math.Abs(delta) * ZoomDominanceRatio
                                       && Math.Abs(delta) < RotationThresholdDegrees;
                    if (!isLikelyZoom)
                        OnRotationDeltaChanged(delta);
                    _lastAngle = angle;
                    _lastDistance = distance;
                    if (!isLikelyZoom)
                        e.Handled = true;
                    break;

                case MotionEventActions.PointerUp:
                case MotionEventActions.Cancel:
                default:
                    IsTwoFingerGesture = false;
                    _isRotating = false;
                    ResetRotationDelta();
                    break;
            }
        }
        else
        {
            _isRotating = false;
        }
    }

    private static float GetAngle(MotionEvent motionEvent)
    {
        var dx = motionEvent.GetX(1) - motionEvent.GetX(0);
        var dy = motionEvent.GetY(1) - motionEvent.GetY(0);
        return (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
    }

    private static float GetDistance(MotionEvent motionEvent)
    {
        var dx = motionEvent.GetX(1) - motionEvent.GetX(0);
        var dy = motionEvent.GetY(1) - motionEvent.GetY(0);
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
}