using System.Drawing;
using System.Numerics;

namespace SceneOrchestrator.Core;

/// <summary>
/// Defines common light properties for scene nodes.
/// </summary>
public interface ILight
{
    /// <summary>
    /// Gets or sets the light intensity multiplier.
    /// </summary>
    float Intensity { get; set; }

    /// <summary>
    /// Gets or sets the light color.
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Gets or sets the light decay factor.
    /// </summary>
    float Decay { get; set; }
}

/// <summary>
/// Supported light source types.
/// </summary>
public enum LightType
{
    /// <summary>
    /// Omnidirectional point light.
    /// </summary>
    Point,

    /// <summary>
    /// Directional light with parallel rays.
    /// </summary>
    Directional,

    /// <summary>
    /// Cone-shaped spot light.
    /// </summary>
    Spot,
}

/// <summary>
/// Scene node representing a light source with type, color, and attenuation settings.
/// </summary>
public class Light(
    string tag,
    Vector3 position,
    Quaternion rotation,
    LightType type,
    float intensity,
    Color color,
    float decay,
    float cutoffAngle = 0.4f
) : SceneNode(tag, position, rotation), ILight
{
    /// <summary>
    /// Gets or sets the light type.
    /// </summary>
    public LightType Type { get; set; } = type;

    /// <summary>
    /// Gets or sets the light intensity multiplier.
    /// </summary>
    public float Intensity { get; set; } = intensity;

    /// <summary>
    /// Gets or sets the light color.
    /// </summary>
    public Color Color { get; set; } = color;

    /// <summary>
    /// Gets or sets the light decay factor.
    /// </summary>
    public float Decay { get; set; } = decay;

    /// <summary>
    /// Gets or sets the spotlight cutoff angle in radians.
    /// </summary>
    public float CutoffAngle { get; set; } = cutoffAngle;
}
