using System.Numerics;
using System.Text.Json.Serialization;

namespace SceneOrchestrator.Core;

/// <summary>
/// Base scene-graph node with a transform, tag, and parent/child links.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SceneNode), "SceneNode")]
[JsonDerivedType(typeof(Camera), "Camera")]
[JsonDerivedType(typeof(Light), "Light")]
[JsonDerivedType(typeof(Mesh), "Mesh")]
public class SceneNode(string tag, Vector3 position, Quaternion rotation)
{
    /// <summary>
    /// Gets the local transform of the node.
    /// </summary>
    [JsonInclude]
    public Transform Transform { get; private set; } = new(position, rotation);

    /// <summary>
    /// Gets the parent node, or null if this is a root node.
    /// </summary>
    [JsonIgnore]
    public SceneNode? Parent { get; protected set; }

    /// <summary>
    /// Gets the list of child nodes.
    /// </summary>
    public List<SceneNode> Children { get; } = [];

    /// <summary>
    /// Gets or sets the node tag used for identification.
    /// </summary>
    public string Tag { get; set; } = tag;

    /// <summary>
    /// Creates a default scene node for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public SceneNode() : this("node", Vector3.Zero, Quaternion.Identity)
    {
    }

    /// <summary>
    /// Adds a child node and assigns this node as its parent.
    /// </summary>
    public void AddChild(SceneNode child)
    {
        if (child.Parent is not null && child.Parent != this)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        if (!Children.Contains(child))
            Children.Add(child);
    }

    /// <summary>
    /// Removes a child node and clears its parent link.
    /// Returns true if the child was found and removed, false otherwise.
    /// </summary>
    public bool RemoveChild(SceneNode child)
    {
        if (!Children.Remove(child))
            return false;

        child.Parent = null;
        return true;
    }

    /// <summary>
    /// Finds the first direct child with the given tag.
    /// </summary>
    public SceneNode? FindChild(string tag)
    {
        return Children.FirstOrDefault(c => c.Tag == tag);
    }

    /// <summary>
    /// Finds the first descendant (including this node) with the given tag.
    /// </summary>
    public SceneNode? FindDescendant(string tag)
    {
        if (Tag == tag)
            return this;
        foreach (var child in Children)
        {
            var found = child.FindDescendant(tag);
            if (found is not null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Finds all descendants (including this node) with the given tag.
    /// </summary>
    public List<SceneNode> FindDescendants(string tag)
    {
        var matches = new List<SceneNode>();
        if (Tag == tag)
            matches.Add(this);
        foreach (var child in Children)
        {
            matches.AddRange(child.FindDescendants(tag));
        }
        return matches;
    }

    /// <summary>
    /// Removes the first matching descendant with the given tag.
    /// Returns true when a node was removed.
    /// </summary>
    public bool RemoveDescendant(string tag)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (child.Tag == tag)
            {
                child.Parent = null;
                Children.RemoveAt(i);
                return true;
            }

            if (child.RemoveDescendant(tag))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Rebuilds parent links for this node and its descendants.
    /// </summary>
    public void RebuildParentLinks(SceneNode? parent = null)
    {
        Parent = parent;
        foreach (var child in Children)
            child.RebuildParentLinks(this);
    }
}
