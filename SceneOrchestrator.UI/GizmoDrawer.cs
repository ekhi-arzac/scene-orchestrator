using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SceneOrchestrator.Core;
using NumMat = System.Numerics.Matrix4x4;

namespace SceneOrchestrator.UI;

/// <summary>
/// Draws editor gizmos and helper visuals.
/// </summary>
internal sealed class GizmoDrawer
{
    /// <summary>
    /// Graphics device used for drawing gizmos.
    /// </summary>
    private readonly GraphicsDevice _device;

    /// <summary>
    /// Wireframe effect used for gizmo lines.
    /// </summary>
    private readonly BasicEffect _wireEffect;

    /// <summary>
    /// Creates a gizmo drawer.
    /// </summary>
    public GizmoDrawer(GraphicsDevice device, BasicEffect wireEffect)
    {
        _device = device;
        _wireEffect = wireEffect;
    }

    /// <summary>
    /// Draws a reference grid in the scene view.
    /// </summary>
    public void DrawGrid(Matrix view, Matrix proj)
    {
        const int half = 10;
        var lines = new List<VertexPositionColor>();
        void L(Vector3 a, Vector3 b, Color c) { lines.Add(new(a, c)); lines.Add(new(b, c)); }

        var dim = new Color(70, 70, 70);
        for (int i = -half; i <= half; i++)
        {
            if (i == 0) continue;
            L(new(i, 0, -half), new(i, 0, half), dim);
            L(new(-half, 0, i), new(half, 0, i), dim);
        }
        L(new(-half, 0, 0), new(half, 0, 0), new Color(150, 50, 50));
        L(new(0, 0, -half), new(0, 0, half), new Color(50, 100, 150));

        SetWire(Matrix.Identity, view, proj);
        DrawLines(lines);
    }

    /// <summary>
    /// Draws simplified gizmos for the scene graph with an optional selection highlight.
    /// </summary>
    public void DrawGizmos(SceneNode node, SceneNode? selected, Matrix view, Matrix proj) =>
        DrawGizmos(node, selected, view, proj, NumMat.Identity);

    /// <summary>
    /// Draws gizmos while accumulating parent transforms.
    /// </summary>
    private void DrawGizmos(
        SceneNode node,
        SceneNode? selected,
        Matrix view,
        Matrix proj,
        NumMat parentModel
    )
    {
        var world = node.Transform.ModelMatrix(parentModel);
        var pos = WorldPos(world);
        var size = node == selected ? 0.25f : 0.18f;

        DrawAxisCross(pos, size, NodeColor(node, selected), view, proj);

        foreach (var child in node.Children)
            DrawGizmos(child, selected, view, proj, world);
    }

    /// <summary>
    /// Draws a three-axis cross at the given world position.
    /// </summary>
    private void DrawAxisCross(Vector3 center, float size, Color color, Matrix view, Matrix proj)
    {
        var lines = new List<VertexPositionColor>();
        void L(Vector3 a, Vector3 b) { lines.Add(new(a, color)); lines.Add(new(b, color)); }

        L(center + new Vector3(size, 0, 0), center - new Vector3(size, 0, 0));
        L(center + new Vector3(0, size, 0), center - new Vector3(0, size, 0));
        L(center + new Vector3(0, 0, size), center - new Vector3(0, 0, size));

        SetWire(Matrix.Identity, view, proj);
        DrawLines(lines);
    }

    /// <summary>
    /// Chooses a color for a scene node gizmo.
    /// </summary>
    private static Color NodeColor(SceneNode node, SceneNode? selected)
    {
        if (node == selected)
            return new Color(255, 140, 0);
        if (node is Light light)
            return new Color(light.Color.R, light.Color.G, light.Color.B);
        if (node is Camera)
            return new Color(220, 220, 80);
        return new Color(180, 180, 180);
    }

    /// <summary>
    /// Configures the wireframe effect matrices.
    /// </summary>
    private void SetWire(Matrix world, Matrix view, Matrix proj)
    {
        _wireEffect.World = world;
        _wireEffect.View = view;
        _wireEffect.Projection = proj;
    }

    /// <summary>
    /// Extracts a world-space position from a matrix.
    /// </summary>
    private static Vector3 WorldPos(NumMat world)
    {
        return new Vector3(world.M41, world.M42, world.M43);
    }

    /// <summary>
    /// Draws a list of line segments.
    /// </summary>
    private void DrawLines(List<VertexPositionColor> lines)
    {
        if (lines.Count < 2) return;
        var arr = lines.ToArray();
        foreach (var pass in _wireEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawUserPrimitives(PrimitiveType.LineList, arr, 0, arr.Length / 2);
        }
    }
}
