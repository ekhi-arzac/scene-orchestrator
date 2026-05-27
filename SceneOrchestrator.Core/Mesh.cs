using System.Numerics;

namespace SceneOrchestrator.Core;

/// <summary>
/// Defines a mesh-backed scene node by its asset path.
/// </summary>
public interface IMesh
{
    /// <summary>
    /// Gets or sets the mesh asset path.
    /// </summary>
    string MeshPath { get; set; }
}

/// <summary>
/// Scene node that references a renderable mesh asset.
/// </summary>
public class Mesh(string tag, Vector3 position, Quaternion rotation, string meshPath)
    : SceneNode(tag, position, rotation),
        IMesh
{
    /// <summary>
    /// Gets or sets the mesh asset path.
    /// </summary>
    public string MeshPath { get; set; } = meshPath;
}
