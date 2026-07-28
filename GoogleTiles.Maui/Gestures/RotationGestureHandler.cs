using SkiaSharp.Views.Maui.Controls;

namespace GoogleTiles.Maui.Gestures;

internal partial class RotationGestureHandler
{
    public event Action<float>? RotationDeltaChanged;
    public event Action? RotationReset;

    private bool _isTwoFinger;

    public bool IsTwoFingerGesture
    {
        get => _isTwoFinger;
        private set => _isTwoFinger = value;
    }

    protected void OnRotationDeltaChanged(float delta) => RotationDeltaChanged?.Invoke(delta);
    protected void ResetRotationDelta() => RotationReset?.Invoke();
}