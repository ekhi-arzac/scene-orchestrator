using System.Numerics;
using SceneOrchestrator.Core;

namespace SceneOrchestrator.Tests;

/// <summary>
/// Unit tests for scene graph node hierarchy behavior.
/// </summary>
public class SceneNodeTests
{
    /// <summary>
    /// Creates a basic scene node for tests.
    /// </summary>
    private static SceneNode Node(string tag = "n") =>
        new(tag, Vector3.Zero, Quaternion.Identity);

    /// <summary>
    /// Verifies AddChild assigns the parent.
    /// </summary>
    [Fact]
    public void AddChild_SetsParent()
    {
        var parent = Node("parent");
        var child  = Node("child");

        parent.AddChild(child);

        Assert.Same(parent, child.Parent);
    }

    /// <summary>
    /// Verifies AddChild adds to the children list.
    /// </summary>
    [Fact]
    public void AddChild_AppearsInChildren()
    {
        var parent = Node();
        var child  = Node("c");

        parent.AddChild(child);

        Assert.Contains(child, parent.Children);
    }

    /// <summary>
    /// Verifies RemoveChild clears the parent link and returns true.
    /// </summary>
    [Fact]
    public void RemoveChild_ClearsParent()
    {
        var parent = Node();
        var child  = Node("c");
        parent.AddChild(child);

        var removed = parent.RemoveChild(child);

        Assert.True(removed);
        Assert.Null(child.Parent);
    }

    /// <summary>
    /// Verifies RemoveChild removes from the children list.
    /// </summary>
    [Fact]
    public void RemoveChild_DisappearsFromChildren()
    {
        var parent = Node();
        var child  = Node("c");
        parent.AddChild(child);

        parent.RemoveChild(child);

        Assert.DoesNotContain(child, parent.Children);
    }

    /// <summary>
    /// Verifies FindChild returns the matching node.
    /// </summary>
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

    /// <summary>
    /// Verifies FindChild returns null when no match exists.
    /// </summary>
    [Fact]
    public void FindChild_ReturnsNullWhenMissing()
    {
        var parent = Node();

        Assert.Null(parent.FindChild("nope"));
    }

    /// <summary>
    /// Verifies a new node has no parent.
    /// </summary>
    [Fact]
    public void NewNode_HasNoParent()
    {
        Assert.Null(Node().Parent);
    }

    /// <summary>
    /// Verifies a new node has no children.
    /// </summary>
    [Fact]
    public void NewNode_HasNoChildren()
    {
        Assert.Empty(Node().Children);
    }

    /// <summary>
    /// Verifies the tag is preserved.
    /// </summary>
    [Fact]
    public void Tag_IsPreserved()
    {
        Assert.Equal("myTag", Node("myTag").Tag);
    }

    /// <summary>
    /// Verifies multiple children are tracked.
    /// </summary>
    [Fact]
    public void MultipleChildren_AllTracked()
    {
        var parent = Node();
        var kids = Enumerable.Range(0, 5).Select(i => Node($"k{i}")).ToList();
        kids.ForEach(parent.AddChild);

        Assert.Equal(5, parent.Children.Count);
        Assert.All(kids, k => Assert.Contains(k, parent.Children));
    }

    /// <summary>
    /// Verifies AddChild updates the parent when reparenting.
    /// </summary>
    [Fact]
    public void AddChild_ReplacesOldParent()
    {
        var p1 = Node("p1");
        var p2 = Node("p2");
        var child = Node("c");

        p1.AddChild(child);
        p2.AddChild(child);

        Assert.Same(p2, child.Parent);
        Assert.DoesNotContain(child, p1.Children);
    }

    /// <summary>
    /// Verifies RemoveChild returns false and preserves the original parent.
    /// </summary>
    [Fact]
    public void RemoveChild_FailsWhenNotDirectChild()
    {
        var p1 = Node("p1");
        var p2 = Node("p2");
        var child = Node("c");
        p1.AddChild(child);

        var removed = p2.RemoveChild(child);

        Assert.False(removed);
        Assert.Same(p1, child.Parent);
        Assert.Contains(child, p1.Children);
        Assert.DoesNotContain(child, p2.Children);
    }

    /// <summary>
    /// Verifies FindDescendant can return the node itself.
    /// </summary>
    [Fact]
    public void FindDescendant_ReturnsSelfWhenTagMatches()
    {
        var root = Node("root");

        var found = root.FindDescendant("root");

        Assert.Same(root, found);
    }

    /// <summary>
    /// Verifies FindDescendant searches through the hierarchy.
    /// </summary>
    [Fact]
    public void FindDescendant_FindsDeepMatch()
    {
        var root = Node("root");
        var child = Node("child");
        var grandchild = Node("target");
        root.AddChild(child);
        child.AddChild(grandchild);

        var found = root.FindDescendant("target");

        Assert.Same(grandchild, found);
    }

    /// <summary>
    /// Verifies FindDescendants returns all matches, including the node itself.
    /// </summary>
    [Fact]
    public void FindDescendants_ReturnsAllMatches()
    {
        var root = Node("match");
        var match1 = Node("match");
        var match2 = Node("match");
        var other = Node("other");
        root.AddChild(match1);
        root.AddChild(other);
        other.AddChild(match2);

        var matches = root.FindDescendants("match");

        Assert.Equal(3, matches.Count);
        Assert.Contains(root, matches);
        Assert.Contains(match1, matches);
        Assert.Contains(match2, matches);
    }

    /// <summary>
    /// Verifies RemoveDescendant removes a matching deep node.
    /// </summary>
    [Fact]
    public void RemoveDescendant_RemovesDeepNode()
    {
        var root = Node("root");
        var child = Node("child");
        var grandchild = Node("target");
        root.AddChild(child);
        child.AddChild(grandchild);

        var removed = root.RemoveDescendant("target");

        Assert.True(removed);
        Assert.Null(grandchild.Parent);
        Assert.DoesNotContain(grandchild, child.Children);
        Assert.Null(root.FindDescendant("target"));
    }

    /// <summary>
    /// Verifies RemoveDescendant does not remove the node itself.
    /// </summary>
    [Fact]
    public void RemoveDescendant_DoesNotRemoveSelf()
    {
        var root = Node("root");

        var removed = root.RemoveDescendant("root");

        Assert.False(removed);
        Assert.Same(root, root.FindDescendant("root"));
    }
}
