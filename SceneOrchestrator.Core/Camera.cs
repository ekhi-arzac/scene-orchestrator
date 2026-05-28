using System.Numerics;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Builds a view matrix from the camera transform.
    /// </summary>
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
    /// <summary>
    /// Creates a default camera for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public Camera()
        : this(
            "Camera",
            Vector3.Zero,
            Quaternion.Identity,
            CameraType.Perspective,
            MathF.PI / 3f,
            0.1f,
            100f,
            16f / 9f
        )
    {
    }

    /// <summary>
    /// Gets or sets the camera projection type.
    /// </summary>
    public CameraType Type { get; set; } = type;

    /// <summary>
    /// Builds a view matrix from the camera transform.
    /// </summary>
    public Matrix4x4 ViewMatrix() => Transform.ViewMatrix();

    /// <summary>
    /// Builds a projection matrix based on the camera type and properties.
    /// Throws ArgumentOutOfRange exception if near plane is further than far plane, or if field of view is non-positive.
    /// </summary>
    [JsonIgnore]
    public Matrix4x4 ProjectionMatrix =>
        Type == CameraType.Perspective
            ? Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane)
            : Matrix4x4.CreateOrthographic(
                FieldOfView * AspectRatio,
                FieldOfView,
                NearPlane,
                FarPlane
            );

    /// <summary>
    /// Gets or sets the vertical field of view in radians.
    /// </summary>
    public float FieldOfView { get; set; } = fieldOfView;

    /// <summary>
    /// Gets or sets the near clipping plane distance.
    /// </summary>
    public float NearPlane { get; set; } = nearPlane;

    /// <summary>
    /// Gets or sets the far clipping plane distance.
    /// </summary>
    public float FarPlane { get; set; } = farPlane;

    /// <summary>
    /// Gets or sets the aspect ratio (width / height).
    /// </summary>
    public float AspectRatio { get; set; } = aspectRatio;
}
