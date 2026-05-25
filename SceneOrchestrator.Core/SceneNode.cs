using System.Numerics;
using System.Text.Json.Serialization;

namespace SceneOrchestrator.Core;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SceneNode), "SceneNode")]
[JsonDerivedType(typeof(Camera), "Camera")]
[JsonDerivedType(typeof(Light), "Light")]
[JsonDerivedType(typeof(Mesh), "Mesh")]
public class SceneNode(string tag, Vector3 position, Quaternion rotation)
{
    public Transform Transform { get; } = new(position, rotation);
    public SceneNode? Parent { get; protected set; }
    public List<SceneNode> Children { get; } = [];

    public string Tag { get; } = tag;

    public void AddChild(SceneNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void RemoveChild(SceneNode child)
    {
        child.Parent = null;
        Children.Remove(child);
    }

    public SceneNode? FindChild(string tag)
    {
        return Children.First(c => c.Tag == tag);
    }
}
