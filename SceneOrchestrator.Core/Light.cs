using System.Drawing;
using System.Numerics;

namespace SceneOrchestrator.Core;

public interface ILight
{
    float Intensity { get; set; }
    Color Color { get; set; }
    float Decay { get; set; }
}

public enum LightType
{
    Point,
    Directional,
    Spot,
}
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
    public LightType Type { get; set; } = type;
    public float Intensity { get; set; } = intensity;
    public Color Color { get; set; } = color;
    public float Decay { get; set; } = decay;
    public float CutoffAngle { get; set; } = cutoffAngle;
}
