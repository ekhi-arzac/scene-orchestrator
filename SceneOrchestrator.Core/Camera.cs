using System.Numerics;

namespace SceneOrchestrator.Core;

/// <summary>
/// Represents an interface for a camera that projects objects in a 3D scene using a projection matrix.
/// </summary>
interface ICamera
{
    public Matrix4x4 ViewMatrix();
    public Matrix4x4 ProjectionMatrix { get; }

    public float FieldOfView { get; }
    public float NearPlane { get; }
    public float FarPlane { get; }
    public float AspectRatio { get; }
}

/// <summary>
/// Represents an abstract camera that projects objects in a 3D scene using a projection matrix.
/// </summary>
public abstract class Camera(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float fieldOfView,
    float nearPlane,
    float farPlane,
    float aspectRatio
) : SceneNode(tag, position, rotation), ICamera
{
    public Matrix4x4 ViewMatrix() => Transform.ViewMatrix();

    public abstract Matrix4x4 ProjectionMatrix { get; protected set; }

    public float FieldOfView { get; protected set; } = fieldOfView;
    public float NearPlane { get; protected set; } = nearPlane;
    public float FarPlane { get; protected set; } = farPlane;
    public float AspectRatio { get; protected set; } = aspectRatio;

    /// <summary>
    /// Updates the perspective projection matrix of the camera with the specified field of view, near plane, far plane, and aspect ratio.
    ///
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="fov"/> is not positive, <paramref name="near"/> is not positive, or <paramref name="far"/> is not positive.
    /// </exception>
    /// </summary>
    public void UpdateProjection(float fov, float near, float far, float aspect)
    {
        FieldOfView = fov;
        NearPlane = near;
        FarPlane = far;
        AspectRatio = aspect;

        ProjectionMatrix = Matrix4x4.CreatePerspective(
            FieldOfView,
            AspectRatio,
            NearPlane,
            FarPlane
        );
    }
}

/// <summary>
/// Represents a perspective camera that projects objects in a 3D scene using a perspective projection.
/// </summary>
public class PerspectiveCamera(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float fieldOfView,
    float nearPlane,
    float farPlane,
    float aspectRatio
) : Camera(tag, position, rotation, fieldOfView, nearPlane, farPlane, aspectRatio)
{
    public override Matrix4x4 ProjectionMatrix { get; protected set; } =
        Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);
}

/// <summary>
/// Represents an orthographic camera that projects objects in a 3D space using an orthographic projection.
/// </summary>
public class OrthographicCamera(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float fieldOfView,
    float nearPlane,
    float farPlane,
    float aspectRatio
) : Camera(tag, position, rotation, fieldOfView, nearPlane, farPlane, aspectRatio)
{
    public override Matrix4x4 ProjectionMatrix { get; protected set; } =
        Matrix4x4.CreateOrthographic(fieldOfView * aspectRatio, fieldOfView, nearPlane, farPlane);
}
