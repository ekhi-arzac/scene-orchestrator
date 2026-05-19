namespace SceneOrchestrator.UI;

using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;
using SceneOrchestrator.Core;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public class UserInterface : Game
{
    private GraphicsDeviceManager _graphicsDeviceManager;
    private ImGuiRenderer _imGuiRendeder;

    private SceneNode _root = new("root", Vector3.Zero, Quaternion.Identity);
    private SceneNode _selectedNode;

    public UserInterface()
    {
        _graphicsDeviceManager = new(this);
        // set cursor  visible
        IsMouseVisible = true;
        _selectedNode = _root;
    }

    protected override void Initialize()
    {
        _imGuiRendeder = new(this);
        _imGuiRendeder.RebuildFontAtlas();
        _root.AddChild(new SceneNode("child1", Vector3.Zero, Quaternion.Identity));
        _root.AddChild(new SceneNode("child2", Vector3.Zero, Quaternion.Identity));
        _root.AddChild(new SceneNode("child3", Vector3.Zero, Quaternion.Identity));
        _root.AddChild(new SceneNode("child4", Vector3.Zero, Quaternion.Identity));
        _root.AddChild(new SceneNode("child5", Vector3.Zero, Quaternion.Identity));
        base.Initialize();
    }

    protected override void Draw(GameTime gameTime)
    {
        _graphicsDeviceManager.GraphicsDevice.Clear(Color.Coral);
        base.Draw(gameTime);

        _imGuiRendeder.BeginLayout(gameTime);
        _drawSceneNode(_root);
        _drawSelectedNode();
        _imGuiRendeder.EndLayout();
    }

    private void _drawSceneNode(SceneNode node)
    {
        if (ImGui.TreeNodeEx(node.Tag))
        {
            if (ImGui.IsItemClicked())
            {
                Console.WriteLine($"Selected node: {node.Tag}");
                _selectedNode = node;
            }

            foreach (var child in node.Children)
            {
                _drawSceneNode(child);
            }
            ImGui.TreePop();
        }
    }

    private void _drawSelectedNode()
    {
        ImGui.Begin("Selected Node");
        ImGui.Text($"Selected node: {_selectedNode.Tag} [{_selectedNode.GetType().Name}]");

        var pos = _selectedNode.Transform.Position;
        if (ImGui.DragFloat3("Position", ref pos, 0.1f))
            _selectedNode.Transform.Position = pos;

        var rot = _selectedNode.Transform.Rotation.AsVector4();
        if (ImGui.DragFloat4("Rotation", ref rot, 0.005f))
            _selectedNode.Transform.Rotation = rot.AsQuaternion();

        var scale = _selectedNode.Transform.Scale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.005f))
            _selectedNode.Transform.Scale = scale;
        ImGui.EndMenu();
    }
}
