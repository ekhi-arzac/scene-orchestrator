using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SceneOrchestrator.Core;
using Matrix = Microsoft.Xna.Framework.Matrix;
using NumMat = System.Numerics.Matrix4x4;

namespace SceneOrchestrator.UI;

internal struct ObjectBuffers
{
    public VertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;
    public int TriangleCount;
}

/// <summary>
/// Renders mesh nodes using a shared placeholder cube.
/// </summary>
/// <remarks>
/// The simplified UI does not load external meshes or lighting.
/// </remarks>
internal sealed class MeshRenderer(GraphicsDevice device, BasicEffect effect) : IDisposable
{
    /// <summary>
    /// Graphics device used for mesh rendering.
    /// </summary>
    private readonly GraphicsDevice _device = device;

    /// <summary>
    /// Shared effect for drawing meshes.
    /// </summary>
    private readonly BasicEffect _effect = effect;

    /// <summary>
    /// Cached unit cube geometry.
    /// </summary>
    private ObjectBuffers _cube;

    /// <summary>
    /// Whether the cube geometry has been initialized.
    /// </summary>
    private bool _cubeReady;

    /// <summary>
    /// Renders all mesh nodes in the scene graph.
    /// </summary>
    public void RenderScene(SceneNode node, Matrix view, Matrix proj) =>
        RenderScene(node, view, proj, NumMat.Identity);

    /// <summary>
    /// Renders the scene graph with a running parent model matrix.
    /// </summary>
    private void RenderScene(SceneNode node, Matrix view, Matrix proj, NumMat parentModel)
    {
        var world = node.Transform.ModelMatrix(parentModel);
        if (node is Mesh)
            DrawMesh(view, proj, world);
        foreach (var child in node.Children)
            RenderScene(child, view, proj, world);
    }

    /// <summary>
    /// Draws a single mesh node using the shared cube geometry.
    /// </summary>
    private void DrawMesh(Matrix view, Matrix proj, NumMat world)
    {
        EnsureCube();

        _effect.World = new Matrix(
            world.M11,
            world.M12,
            world.M13,
            world.M14,
            world.M21,
            world.M22,
            world.M23,
            world.M24,
            world.M31,
            world.M32,
            world.M33,
            world.M34,
            world.M41,
            world.M42,
            world.M43,
            world.M44
        );
        _effect.View = view;
        _effect.Projection = proj;
        _effect.LightingEnabled = false;
        _effect.VertexColorEnabled = true;

        _device.SetVertexBuffer(_cube.VertexBuffer);
        _device.Indices = _cube.IndexBuffer;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _cube.TriangleCount);
        }
    }

    /// <summary>
    /// Ensures the cube geometry is available for rendering.
    /// </summary>
    private void EnsureCube()
    {
        if (_cubeReady)
            return;
        _cube = BuildCube(_device);
        _cubeReady = true;
    }

    /// <summary>
    /// Builds a unit cube mesh with per-vertex color.
    /// </summary>
    private static ObjectBuffers BuildCube(GraphicsDevice device)
    {
        var c = Color.White;
        var verts = new VertexPositionColor[]
        {
            new(new(-0.5f, -0.5f, -0.5f), c), // 0
            new(new(0.5f, -0.5f, -0.5f), c), // 1
            new(new(0.5f, 0.5f, -0.5f), c), // 2
            new(new(-0.5f, 0.5f, -0.5f), c), // 3
            new(new(-0.5f, -0.5f, 0.5f), c), // 4
            new(new(0.5f, -0.5f, 0.5f), c), // 5
            new(new(0.5f, 0.5f, 0.5f), c), // 6
            new(new(-0.5f, 0.5f, 0.5f), c), // 7
        };

        short[] idx =
        [
            // Front (+Z)
            4,
            5,
            6,
            4,
            6,
            7,
            // Back (-Z)
            1,
            0,
            3,
            1,
            3,
            2,
            // Left (-X)
            0,
            4,
            7,
            0,
            7,
            3,
            // Right (+X)
            5,
            1,
            2,
            5,
            2,
            6,
            // Top (+Y)
            3,
            7,
            6,
            3,
            6,
            2,
            // Bottom (-Y)
            0,
            1,
            5,
            0,
            5,
            4,
        ];

        var vb = new VertexBuffer(
            device,
            typeof(VertexPositionColor),
            verts.Length,
            BufferUsage.WriteOnly
        );
        vb.SetData(verts);

        var ib = new IndexBuffer(
            device,
            IndexElementSize.SixteenBits,
            idx.Length,
            BufferUsage.WriteOnly
        );
        ib.SetData(idx);

        return new ObjectBuffers
        {
            VertexBuffer = vb,
            IndexBuffer = ib,
            TriangleCount = idx.Length / 3,
        };
    }

    /// <summary>
    /// Disposes cached GPU buffers.
    /// </summary>
    public void Dispose()
    {
        if (_cubeReady)
        {
            _cube.VertexBuffer.Dispose();
            _cube.IndexBuffer.Dispose();
        }
    }
}
