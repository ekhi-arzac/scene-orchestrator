using System.Drawing;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SceneOrchestrator.Core;

/// <summary>
/// JSON converter for System.Drawing.Color.
/// </summary>
public class ColorConverter : JsonConverter<Color>
{
    /// <summary>
    /// Reads a Color from the JSON reader.
    /// </summary>
    public override Color Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of Color object.");

        int r = 0, g = 0, b = 0, a = 255;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name for Color.");

            var name = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end when reading Color.");

            switch (name)
            {
                case "r":
                case "R":
                    r = reader.GetInt32();
                    break;
                case "g":
                case "G":
                    g = reader.GetInt32();
                    break;
                case "b":
                case "B":
                    b = reader.GetInt32();
                    break;
                case "a":
                case "A":
                    a = reader.GetInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Writes a Color to JSON using r/g/b/a properties.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("r", value.R);
        writer.WriteNumber("g", value.G);
        writer.WriteNumber("b", value.B);
        writer.WriteNumber("a", value.A);
        writer.WriteEndObject();
    }
}

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
    /// Creates a default light for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public Light()
        : this(
            "Light",
            Vector3.Zero,
            Quaternion.Identity,
            LightType.Point,
            1f,
            Color.White,
            0.3f
        )
    {
    }

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
    [JsonConverter(typeof(ColorConverter))]
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
