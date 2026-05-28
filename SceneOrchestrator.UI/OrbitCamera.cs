using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SceneOrchestrator.UI;

/// <summary>
/// Simple orbit camera controller for the editor viewport.
/// </summary>
internal sealed class OrbitCamera
{
    /// <summary>
    /// Current orbit target.
    /// </summary>
    private Vector3 _target = Vector3.Zero;

    /// <summary>
    /// Distance from the target.
    /// </summary>
    private float _dist = 10f;

    /// <summary>
    /// Yaw angle in radians.
    /// </summary>
    private float _yaw = 0.5f;

    /// <summary>
    /// Pitch angle in radians.
    /// </summary>
    private float _pitch = 0.4f;

    /// <summary>
    /// Previous mouse state for delta calculations.
    /// </summary>
    private MouseState _prev;

    /// <summary>
    /// Updates orbit and pan input based on mouse state.
    /// </summary>
    public void UpdateInput(MouseState mouse, bool hovering)
    {
        if (hovering)
        {
            if (
                mouse.RightButton == ButtonState.Pressed
                && _prev.RightButton == ButtonState.Pressed
            )
            {
                _yaw += (mouse.X - _prev.X) * 0.006f;
                _pitch = MathHelper.Clamp(_pitch - (mouse.Y - _prev.Y) * 0.006f, -1.5f, 1.5f);
            }

            if (
                mouse.MiddleButton == ButtonState.Pressed
                && _prev.MiddleButton == ButtonState.Pressed
            )
            {
                var view = ViewMatrix();
                var right = new Vector3(view.M11, view.M21, view.M31);
                var up = new Vector3(view.M12, view.M22, view.M32);
                float s = _dist * 0.001f;
                _target -= right * (mouse.X - _prev.X) * s;
                _target += up * (mouse.Y - _prev.Y) * s;
            }

            int scroll = mouse.ScrollWheelValue - _prev.ScrollWheelValue;
            _dist = MathHelper.Clamp(_dist - scroll * 0.01f, 0.3f, 500f);
        }
        _prev = mouse;
    }

    /// <summary>
    /// Sets the orbit target to the given world position.
    /// </summary>
    public void FocusOn(System.Numerics.Vector3 pos) => _target = new Vector3(pos.X, pos.Y, pos.Z);

    /// <summary>
    /// Builds the camera view matrix.
    /// </summary>
    public Matrix ViewMatrix()
    {
        var camPos =
            _target
            + new Vector3(
                MathF.Sin(_yaw) * MathF.Cos(_pitch) * _dist,
                MathF.Sin(_pitch) * _dist,
                MathF.Cos(_yaw) * MathF.Cos(_pitch) * _dist
            );
        return Matrix.CreateLookAt(camPos, _target, Vector3.Up);
    }

    /// <summary>
    /// Builds the camera projection matrix.
    /// </summary>
    public Matrix ProjMatrix(float aspect) =>
        Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 0.01f, 1000f);
}
