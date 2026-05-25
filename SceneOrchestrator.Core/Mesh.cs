using System.Numerics;

namespace SceneOrchestrator.Core;

public interface IMesh
{
    string MeshPath { get; set; }
}

public class Mesh(string tag, Vector3 position, Quaternion rotation, string meshPath)
    : SceneNode(tag, position, rotation),
        IMesh
{
    public string MeshPath { get; set; } = meshPath;
}
