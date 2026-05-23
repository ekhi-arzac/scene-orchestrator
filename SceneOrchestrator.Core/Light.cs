using System.Drawing;
using System.Numerics;

namespace SceneOrchestrator.Core;

public interface ILight
{
    float Intensity { get; set; }
    Color Color { get; set; }
    float Decay { get; set; }
}

public abstract class Light(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float intensity,
    Color color,
    float decay
) : SceneNode(tag, position, rotation)
{
    public float Intensity { get; set; } = intensity;
    public Color Color { get; set; } = color;
    public float Decay { get; set; } = decay;
}

public class PointLight(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float intensity,
    Color color,
    float decay
) : Light(tag, position, rotation, intensity, color, decay) { }

public class DirectionalLight(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float intensity,
    Color color,
    float decay
) : Light(tag, position, rotation, intensity, color, decay) { }

public class SpotLight(
    string tag,
    Vector3 position,
    Quaternion rotation,
    float intensity,
    Color color,
    float decay,
    float cutoffAngle
) : Light(tag, position, rotation, intensity, color, decay)
{
    public float CutoffAngle { get; set; } = cutoffAngle;
}
