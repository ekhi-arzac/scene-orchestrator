using System.Numerics;

namespace SceneOrchestrator.Core;

public enum CameraType
{
    Perspective,
    Orthographic,
}

/// <summary>
/// Represents an interface for a camera that projects objects in a 3D scene using a projection matrix.
/// </summary>
interface ICamera
{
    public CameraType Type { get; }
    public Matrix4x4 ViewMatrix();
    public Matrix4x4 ProjectionMatrix { get; }

    public float FieldOfView { get; }
    public float NearPlane { get; }
    public float FarPlane { get; }
    public float AspectRatio { get; }
}

/// <summary>
/// Represents a camera that projects objects in a 3D scene using a projection matrix.
/// </summary>
public class Camera(
    string tag,
    Vector3 position,
    Quaternion rotation,
    CameraType type,
    float fieldOfView,
    float nearPlane,
    float farPlane,
    float aspectRatio
) : SceneNode(tag, position, rotation), ICamera
{
    public CameraType Type { get; set; } = type;

    public Matrix4x4 ViewMatrix() => Transform.ViewMatrix();

    public Matrix4x4 ProjectionMatrix => Type == CameraType.Perspective
        ? Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane)
        : Matrix4x4.CreateOrthographic(FieldOfView * AspectRatio, FieldOfView, NearPlane, FarPlane);

    public float FieldOfView { get; set; } = fieldOfView;
    public float NearPlane { get; set; } = nearPlane;
    public float FarPlane { get; set; } = farPlane;
    public float AspectRatio { get; set; } = aspectRatio;
}
