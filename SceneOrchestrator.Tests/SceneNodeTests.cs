using System.Numerics;
using SceneOrchestrator.Core;

namespace SceneOrchestrator.Tests;

public class SceneNodeTests
{
    private static SceneNode Node(string tag = "n") =>
        new(tag, Vector3.Zero, Quaternion.Identity);

    [Fact]
    public void AddChild_SetsParent()
    {
        var parent = Node("parent");
        var child  = Node("child");

        parent.AddChild(child);

        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void AddChild_AppearsInChildren()
    {
        var parent = Node();
        var child  = Node("c");

        parent.AddChild(child);

        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void RemoveChild_ClearsParent()
    {
        var parent = Node();
        var child  = Node("c");
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.Null(child.Parent);
    }

    [Fact]
    public void RemoveChild_DisappearsFromChildren()
    {
        var parent = Node();
        var child  = Node("c");
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.DoesNotContain(child, parent.Children);
    }

    [Fact]
    public void FindChild_ReturnsCorrectNode()
    {
        var parent = Node();
        var a = Node("alpha");
        var b = Node("beta");
        parent.AddChild(a);
        parent.AddChild(b);

        var found = parent.FindChild("beta");

        Assert.Same(b, found);
    }

    [Fact]
    public void FindChild_ThrowsWhenMissing()
    {
        var parent = Node();

        Assert.Throws<InvalidOperationException>(() => parent.FindChild("nope"));
    }

    [Fact]
    public void NewNode_HasNoParent()
    {
        Assert.Null(Node().Parent);
    }

    [Fact]
    public void NewNode_HasNoChildren()
    {
        Assert.Empty(Node().Children);
    }

    [Fact]
    public void Tag_IsPreserved()
    {
        Assert.Equal("myTag", Node("myTag").Tag);
    }

    [Fact]
    public void MultipleChildren_AllTracked()
    {
        var parent = Node();
        var kids = Enumerable.Range(0, 5).Select(i => Node($"k{i}")).ToList();
        kids.ForEach(parent.AddChild);

        Assert.Equal(5, parent.Children.Count);
        Assert.All(kids, k => Assert.Contains(k, parent.Children));
    }

    [Fact]
    public void AddChild_ReplacesOldParent()
    {
        var p1 = Node("p1");
        var p2 = Node("p2");
        var child = Node("c");

        p1.AddChild(child);
        p2.AddChild(child);

        Assert.Same(p2, child.Parent);
    }
}
