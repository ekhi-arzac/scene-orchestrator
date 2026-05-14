namespace SceneOrchestrator.Core;



public class SceneNode
{
    public Transform Transform { get; } = new();
    public SceneNode? Parent { get; protected set; }
    public List<SceneNode> Children { get; } = [];
    public String Tag { get; set; } = "";

    public static int NextID { get; private set; } = 0;
    public int ID { get; } = NextID++;

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
        return FindChild(c => c.Tag == tag);
    }

    public SceneNode? FindChild(int id)
    {
        return FindChild(c => c.ID == id);
    }

    public SceneNode? FindChild(Func<SceneNode, bool> predicate)
    {
        return Children.FirstOrDefault(predicate);
    }

}
