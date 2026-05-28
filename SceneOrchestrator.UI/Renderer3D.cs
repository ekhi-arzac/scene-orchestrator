using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SceneOrchestrator.Core;

namespace SceneOrchestrator.UI;

/// <summary>
/// Orchestrates the viewport render target, camera, gizmos, and mesh rendering.
/// </summary>
public sealed class Renderer3D : IDisposable
{
    private GraphicsDevice _device = null!;
    private BasicEffect _meshEffect = null!;
    private BasicEffect _wireEffect = null!;
    private RenderTarget2D _viewportRT = null!;

    private static readonly RasterizerState _solidRaster = new() { CullMode = CullMode.None };

    private readonly OrbitCamera _camera = new();

    private GizmoDrawer _gizmos = null!;

    private MeshRenderer _meshRenderer = null!;

    public RenderTarget2D ViewportRT => _viewportRT;

    /// <summary>
    /// Gets the viewport width.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the viewport height.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Initializes renderer resources and creates the viewport render target.
    /// </summary>
    public void Initialize(GraphicsDevice device, int w, int h)
    {
        _device = device;

        _meshEffect = new BasicEffect(device)
        {
            LightingEnabled = false,
            VertexColorEnabled = true,
        };

        _wireEffect = new BasicEffect(device)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
        };

        _gizmos = new GizmoDrawer(device, _wireEffect);
        _meshRenderer = new MeshRenderer(device, _meshEffect);

        Resize(w, h);
    }

    /// <summary>
    /// Resizes the viewport render target.
    /// Throws ArgumentOutOfRange if the dimensions are non-positive
    /// Throws ArgumentNullException if the renderer is not initialized.
    /// </summary>
    public void Resize(int w, int h)
    {
        if (w <= 0 || h <= 0)
            return;
        Width = w;
        Height = h;
        _viewportRT?.Dispose();
        _viewportRT = new RenderTarget2D(
            _device,
            w,
            h,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24
        );
    }

    /// <summary>
    /// Updates camera input handling for the current frame.
    /// </summary>
    public void UpdateInput(MouseState mouse, bool hovering) =>
        _camera.UpdateInput(mouse, hovering);

    /// <summary>
    /// Renders the scene and gizmos into the viewport render target.
    /// </summary>
    public void Render(SceneNode root, SceneNode? selected)
    {
        var view = _camera.ViewMatrix();
        var proj = _camera.ProjMatrix((float)Width / Height);

        _device.SetRenderTarget(_viewportRT);
        _device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(56, 56, 56), 1f, 0);
        _device.RasterizerState = _solidRaster;

        _gizmos.DrawGrid(view, proj);
        _meshRenderer.RenderScene(root, view, proj);
        _gizmos.DrawGizmos(root, selected, view, proj);

        _device.SetRenderTarget(null);
    }

    /// <summary>
    /// Releases GPU resources held by the renderer.
    /// </summary>
    public void Dispose()
    {
        _viewportRT?.Dispose();
        _meshEffect?.Dispose();
        _wireEffect?.Dispose();
        _meshRenderer?.Dispose();
    }
}
