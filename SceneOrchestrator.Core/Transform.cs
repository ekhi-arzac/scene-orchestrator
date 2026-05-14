using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SceneOrchestrator.Core;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return new Vector3(reader.GetSingle(), reader.GetSingle(), reader.GetSingle());
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}

public class QuaternionConverter : JsonConverter<Quaternion>
{
    public override Quaternion Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return new Quaternion(
            reader.GetSingle(),
            reader.GetSingle(),
            reader.GetSingle(),
            reader.GetSingle()
        );
    }

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

public class Transform
{
    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 Position { get; set; } = Vector3.Zero;

    [JsonConverter(typeof(QuaternionConverter))]
    public Quaternion Rotation { get; set; } = Quaternion.Identity;

    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 Scale { get; set; } = Vector3.One;

    public override string ToString()
    {
        return $"Position: {Position}, Rotation: {Rotation}, Scale: {Scale}";
    }

    public Matrix4x4 ModelMatrix(Matrix4x4 parentMatrix)
    {
        return parentMatrix
            * Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateFromQuaternion(Rotation)
            * Matrix4x4.CreateTranslation(Position);
    }

    public Matrix4x4 ViewMatrix()
    {
        return Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
    }
}
