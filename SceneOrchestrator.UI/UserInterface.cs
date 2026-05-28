namespace SceneOrchestrator.UI;

using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using SceneOrchestrator.Core;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using XnaColor = Microsoft.Xna.Framework.Color;

/// <summary>
/// MonoGame host for the simplified editor UI.
/// </summary>
public class UserInterface : Game
{
    /// <summary>
    /// Manages graphics device settings.
    /// </summary>
    private readonly GraphicsDeviceManager _graphics;

    /// <summary>
    /// ImGui renderer wrapper.
    /// </summary>
    private ImGuiRenderer _imGui = null!;

    /// <summary>
    /// 3D renderer for the viewport.
    /// </summary>
    private Renderer3D _renderer = null!;

    /// <summary>
    /// ImGui texture ID for the viewport render target.
    /// </summary>
    private IntPtr _viewportTexId;

    /// <summary>
    /// Root node of the scene graph.
    /// </summary>
    private SceneNode _root = new("root", Vector3.Zero, Quaternion.Identity);

    /// <summary>
    /// Currently selected scene node.
    /// </summary>
    private SceneNode? _selectedNode;

    /// <summary>
    /// Whether the viewport window is hovered.
    /// </summary>
    private bool _viewportHovered;

    /// <summary>
    /// Status message shown after an export attempt.
    /// </summary>
    private string _exportStatus = "";

    /// <summary>
    /// Initializes the editor window and graphics settings.
    /// </summary>
    public UserInterface()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1440,
            PreferredBackBufferHeight = 900,
        };
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    /// <summary>
    /// Creates ImGui and renderer resources and loads the default scene.
    /// </summary>
    protected override void Initialize()
    {
        _imGui = new ImGuiRenderer(this);
        _imGui.RebuildFontAtlas();

        _renderer = new Renderer3D();
        _renderer.Initialize(GraphicsDevice, 800, 600);
        _viewportTexId = _imGui.BindTexture(_renderer.ViewportRT);

        BuildDefaultScene();
        base.Initialize();
    }

    /// <summary>
    /// Builds a small starter scene for the editor.
    /// </summary>
    private void BuildDefaultScene()
    {
        _root = new SceneNode("root", Vector3.Zero, Quaternion.Identity);
        _root.AddChild(CreateCamera("Camera"));
        _root.AddChild(CreateMesh("Mesh"));
        _root.AddChild(CreatePointLight("Light"));
        _selectedNode = _root;
    }

    /// <summary>
    /// Handles editor input and camera manipulation.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        _renderer.UpdateInput(mouse, _viewportHovered);
        base.Update(gameTime);
    }

    /// <summary>
    /// Renders the scene and all UI panels.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        _renderer.Render(_root, _selectedNode);

        GraphicsDevice.Clear(XnaColor.Black);

        _imGui.BeginLayout(gameTime);
        DrawScenePanel();
        DrawInspectorPanel();
        DrawViewportPanel();
        _imGui.EndLayout();

        base.Draw(gameTime);
    }

    // ── Layout ───────────────────────────────────────────────────────────────
    private const float ScenePanelW = 260f;
    private const float InspectorPanelW = 300f;
    private const ImGuiWindowFlags StaticWindow =
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoCollapse;

    // ── Scene panel ──────────────────────────────────────────────────────────
    /// <summary>
    /// Draws the scene hierarchy panel and action buttons.
    /// </summary>
    private void DrawScenePanel()
    {
        var h = ImGui.GetIO().DisplaySize.Y;
        ImGui.SetNextWindowPos(new Vector2(0, 0));
        ImGui.SetNextWindowSize(new Vector2(ScenePanelW, h));
        ImGui.Begin("Scene", StaticWindow);
        DrawSceneActions();
        ImGui.Separator();
        DrawNodeTree(_root);
        ImGui.End();
    }

    /// <summary>
    /// Draws the action buttons for scene management.
    /// </summary>
    private void DrawSceneActions()
    {
        if (ImGui.Button("Reset Scene"))
            BuildDefaultScene();

        ImGui.SameLine();
        if (ImGui.Button("Add Mesh"))
            AddNode(CreateMesh("Mesh"));

        ImGui.SameLine();
        if (ImGui.Button("Add Camera"))
            AddNode(CreateCamera("Camera"));

        ImGui.SameLine();
        if (ImGui.Button("Add Light"))
            AddNode(CreatePointLight("Light"));

        if (ImGui.Button("Export JSON"))
            ExportScene();

        if (_exportStatus != "")
            ImGui.TextDisabled(_exportStatus);
    }

    /// <summary>
    /// Serialises the scene graph to scene.json in the working directory.
    /// </summary>
    private void ExportScene()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_root, options);
            File.WriteAllText("scene.json", json);
            _exportStatus = "Saved to scene.json";
        }
        catch (Exception ex)
        {
            _exportStatus = $"Export failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Adds a node as a child of the current selection (or root).
    /// </summary>
    private void AddNode(SceneNode node)
    {
        var parent = _selectedNode ?? _root;
        parent.AddChild(node);
        _selectedNode = node;
    }

    /// <summary>
    /// Creates a default camera node.
    /// </summary>
    private static Camera CreateCamera(string tag) =>
        new(
            tag,
            new Vector3(0, 2, 5),
            Quaternion.Identity,
            CameraType.Perspective,
            MathF.PI / 3f,
            0.1f,
            100f,
            16f / 9f
        );

    /// <summary>
    /// Creates a default mesh node.
    /// </summary>
    private static Mesh CreateMesh(string tag) =>
        new(tag, Vector3.Zero, Quaternion.Identity, "");

    /// <summary>
    /// Creates a default point light node.
    /// </summary>
    private static Light CreatePointLight(string tag) =>
        new(
            tag,
            new Vector3(3, 5, 3),
            Quaternion.Identity,
            LightType.Point,
            1.0f,
            System.Drawing.Color.White,
            0.3f
        );

    /// <summary>
    /// Recursively draws a node and its children in the hierarchy tree.
    /// </summary>
    private void DrawNodeTree(SceneNode node)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (node.Children.Count == 0)
            flags |= ImGuiTreeNodeFlags.Leaf;
        if (_selectedNode == node)
            flags |= ImGuiTreeNodeFlags.Selected;

        bool open = ImGui.TreeNodeEx($"{NodeLabel(node)}###{node.GetHashCode()}", flags);
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            _selectedNode = node;

        if (open)
        {
            foreach (var child in node.Children.ToArray())
                DrawNodeTree(child);
            ImGui.TreePop();
        }
    }

    /// <summary>
    /// Creates a short label for a node type.
    /// </summary>
    private static string NodeLabel(SceneNode node)
    {
        var prefix = node switch
        {
            Camera => "CAM",
            Light => "LIT",
            Mesh => "MSH",
            _ => "NODE",
        };
        return $"[{prefix}] {node.Tag}";
    }

    // ── Inspector ────────────────────────────────────────────────────────────
    /// <summary>
    /// Draws the inspector panel for the selected node.
    /// </summary>
    private void DrawInspectorPanel()
    {
        var display = ImGui.GetIO().DisplaySize;
        ImGui.SetNextWindowPos(new Vector2(display.X - InspectorPanelW, 0));
        ImGui.SetNextWindowSize(new Vector2(InspectorPanelW, display.Y));
        ImGui.Begin("Inspector", StaticWindow);
        if (_selectedNode == null)
        {
            ImGui.TextDisabled("Nothing selected");
            ImGui.End();
            return;
        }

        DrawSelectionHeader(_selectedNode);
        DrawTransformEditor(_selectedNode.Transform);
        DrawNodeSpecificEditor(_selectedNode);

        if (_selectedNode != _root && ImGui.Button("Delete Node"))
        {
            _selectedNode.Parent?.RemoveChild(_selectedNode);
            _selectedNode = _root;
        }

        ImGui.End();
    }

    /// <summary>
    /// Draws the selection header and tag editor.
    /// </summary>
    private static void DrawSelectionHeader(SceneNode node)
    {
        ImGui.TextDisabled(node.GetType().Name);
        var tag = node.Tag;
        if (ImGui.InputText("Tag", ref tag, 128))
            node.Tag = tag;
        ImGui.Separator();
    }

    /// <summary>
    /// Draws transform controls for a scene node.
    /// </summary>
    private static void DrawTransformEditor(Transform transform)
    {
        var pos = transform.Position;
        if (ImGui.DragFloat3("Position", ref pos, 0.05f))
            transform.Position = pos;

        var rot = transform.Rotation;
        if (DrawQuaternion("Rotation", ref rot, 0.01f))
            transform.Rotation = rot;

        var scale = transform.Scale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.05f))
            transform.Scale = scale;

        ImGui.Separator();
    }

    /// <summary>
    /// Draws a normalized quaternion editor.
    /// </summary>
    private static bool DrawQuaternion(string label, ref Quaternion value, float speed)
    {
        var v = new Vector4(value.X, value.Y, value.Z, value.W);
        if (!ImGui.DragFloat4(label, ref v, speed))
            return false;
        value = Quaternion.Normalize(new Quaternion(v.X, v.Y, v.Z, v.W));
        return true;
    }

    /// <summary>
    /// Draws type-specific fields for the selected node.
    /// </summary>
    private static void DrawNodeSpecificEditor(SceneNode node)
    {
        switch (node)
        {
            case Camera cam:
                DrawCameraEditor(cam);
                break;
            case Light light:
                DrawLightEditor(light);
                break;
            case Mesh mesh:
                DrawMeshEditor(mesh);
                break;
        }
    }

    /// <summary>
    /// Draws camera-specific controls.
    /// </summary>
    private static void DrawCameraEditor(Camera cam)
    {
        ImGui.TextDisabled("Camera");
        if (DrawEnumCombo("Projection", cam.Type, out CameraType newType))
            cam.Type = newType;

        var fov = cam.FieldOfView;
        if (ImGui.DragFloat("Field Of View", ref fov, 0.01f, 0.01f, MathF.PI))
            cam.FieldOfView = fov;

        var near = cam.NearPlane;
        if (ImGui.DragFloat("Near Plane", ref near, 0.01f, 0.01f, 1000f))
            cam.NearPlane = near;

        var far = cam.FarPlane;
        if (ImGui.DragFloat("Far Plane", ref far, 0.1f, 0.1f, 5000f))
            cam.FarPlane = far;

        var aspect = cam.AspectRatio;
        if (ImGui.DragFloat("Aspect Ratio", ref aspect, 0.01f, 0.1f, 4f))
            cam.AspectRatio = aspect;
    }

    /// <summary>
    /// Draws light-specific controls.
    /// </summary>
    private static void DrawLightEditor(Light light)
    {
        ImGui.TextDisabled("Light");
        if (DrawEnumCombo("Type", light.Type, out LightType newType))
            light.Type = newType;

        var intensity = light.Intensity;
        if (ImGui.DragFloat("Intensity", ref intensity, 0.05f, 0f, 10f))
            light.Intensity = intensity;

        var decay = light.Decay;
        if (ImGui.DragFloat("Decay", ref decay, 0.01f, 0f, 10f))
            light.Decay = decay;

        var color = new Vector3(
            light.Color.R / 255f,
            light.Color.G / 255f,
            light.Color.B / 255f
        );
        if (ImGui.ColorEdit3("Color", ref color))
        {
            light.Color = System.Drawing.Color.FromArgb(
                255,
                (int)(color.X * 255f),
                (int)(color.Y * 255f),
                (int)(color.Z * 255f)
            );
        }

        if (light.Type == LightType.Spot)
        {
            var cutoff = light.CutoffAngle;
            if (ImGui.DragFloat("Cutoff Angle", ref cutoff, 0.01f, 0.01f, MathF.PI))
                light.CutoffAngle = cutoff;
        }
    }

    /// <summary>
    /// Draws mesh metadata controls.
    /// </summary>
    private static void DrawMeshEditor(Mesh mesh)
    {
        ImGui.TextDisabled("Mesh");
        var path = mesh.MeshPath;
        if (ImGui.InputText("Mesh Path", ref path, 256))
            mesh.MeshPath = path;
    }

    /// <summary>
    /// Shows a combo box for selecting enum values.
    /// </summary>
    private static bool DrawEnumCombo<T>(string label, T value, out T result) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        var names = Enum.GetNames<T>();
        int idx = Array.IndexOf(values, value);
        if (idx < 0) idx = 0;
        if (ImGui.Combo(label, ref idx, names, names.Length))
        {
            result = values[idx];
            return true;
        }
        result = value;
        return false;
    }

    // ── Viewport ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Draws the 3D viewport and handles resizing.
    /// </summary>
    private void DrawViewportPanel()
    {
        var display = ImGui.GetIO().DisplaySize;
        float vpX = ScenePanelW;
        float vpW = display.X - ScenePanelW - InspectorPanelW;
        ImGui.SetNextWindowPos(new Vector2(vpX, 0));
        ImGui.SetNextWindowSize(new Vector2(vpW, display.Y));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("Viewport", StaticWindow);
        ImGui.PopStyleVar();

        _viewportHovered = ImGui.IsWindowHovered();
        var size = ImGui.GetContentRegionAvail();

        if (size.X > 1 && size.Y > 1)
        {
            int iw = (int)size.X;
            int ih = (int)size.Y;
            if (iw != _renderer.Width || ih != _renderer.Height)
            {
                _imGui.UnbindTexture(_viewportTexId);
                _renderer.Resize(iw, ih);
                _viewportTexId = _imGui.BindTexture(_renderer.ViewportRT);
            }

            ImGui.Image(_viewportTexId, size);
        }

        ImGui.End();
    }

    /// <summary>
    /// Releases renderer resources when the game shuts down.
    /// </summary>
    protected override void UnloadContent()
    {
        _renderer?.Dispose();
        base.UnloadContent();
    }
}
