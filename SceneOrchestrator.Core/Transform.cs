using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SceneOrchestrator.Core;

/// <summary>
/// JSON converter for System.Numerics.Vector3.
/// </summary>
public class Vector3Converter : JsonConverter<Vector3>
{
    /// <summary>
    /// Reads a Vector3 from the JSON reader.
    /// </summary>
    public override Vector3 Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of Vector3 object.");

        float x = 0f,
            y = 0f,
            z = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name for Vector3.");

            var name = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end when reading Vector3.");

            switch (name)
            {
                case "x":
                case "X":
                    x = reader.GetSingle();
                    break;
                case "y":
                case "Y":
                    y = reader.GetSingle();
                    break;
                case "z":
                case "Z":
                    z = reader.GetSingle();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Writes a Vector3 to JSON using x/y/z properties.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}

/// <summary>
/// JSON converter for System.Numerics.Quaternion.
/// </summary>
public class QuaternionConverter : JsonConverter<Quaternion>
{
    /// <summary>
    /// Reads a Quaternion from the JSON reader.
    /// </summary>
    public override Quaternion Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of Quaternion object.");

        float x = 0f,
            y = 0f,
            z = 0f,
            w = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name for Quaternion.");

            var name = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end when reading Quaternion.");

            switch (name)
            {
                case "x":
                case "X":
                    x = reader.GetSingle();
                    break;
                case "y":
                case "Y":
                    y = reader.GetSingle();
                    break;
                case "z":
                case "Z":
                    z = reader.GetSingle();
                    break;
                case "w":
                case "W":
                    w = reader.GetSingle();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new Quaternion(x, y, z, w);
    }

    /// <summary>
    /// Writes a Quaternion to JSON using x/y/z/w properties.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        Quaternion value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("w", value.W);
        writer.WriteEndObject();
    }
}

/// <summary>
/// Local transform with position, rotation, and scale.
/// </summary>
public class Transform(Vector3 position, Quaternion rotation)
{
    /// <summary>
    /// Creates a default transform for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public Transform()
        : this(Vector3.Zero, Quaternion.Identity) { }

    /// <summary>
    /// Gets or sets the local position.
    /// </summary>
    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 Position { get; set; } = position;

    /// <summary>
    /// Gets or sets the local rotation.
    /// </summary>
    [JsonConverter(typeof(QuaternionConverter))]
    public Quaternion Rotation { get; set; } = rotation;

    /// <summary>
    /// Gets or sets the local scale.
    /// </summary>
    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 Scale { get; set; } = Vector3.One;

    /// <summary>
    /// Returns a readable representation of the transform state.
    /// </summary>
    public override string ToString()
    {
        return $"Position: {Position}, Rotation: {Rotation}, Scale: {Scale}";
    }

    /// <summary>
    /// Builds a model matrix, composed with a parent matrix.
    /// </summary>
    public Matrix4x4 ModelMatrix(Matrix4x4 parentMatrix)
    {
        return Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateFromQuaternion(Rotation)
            * Matrix4x4.CreateTranslation(Position)
            * parentMatrix;
    }

    /// <summary>
    /// Builds a view matrix by inverting the local model matrix.
    /// </summary>
    public Matrix4x4 ViewMatrix()
    {
        if (Matrix4x4.Invert(ModelMatrix(Matrix4x4.Identity), out var result))
        {
            return result;
        }
        return Matrix4x4.Identity;
    }

    /// <summary>
    /// Translates the transform in local or world space.
    /// </summary>
    public void Translate(Vector3 translation, bool local)
    {
        if (local)
            Position += Vector3.Transform(translation, Rotation);
        else
            Position += translation;
    }

    /// <summary>
    /// Rotates the transform around an axis in local or world space.
    /// </summary>
    public void Rotate(Vector3 axis, float angleRadians, bool local = false)
    {
        if (local)
            axis = Vector3.Transform(axis, Rotation);

        var q = Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), angleRadians);
        Rotation = Quaternion.Normalize(q * Rotation);
    }
}
