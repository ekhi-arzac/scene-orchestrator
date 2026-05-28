# Specification of the final project for relevant C# courses
(When modifying this document, please maintain the layout and structure and follow the inline instructions.)

## C# Courses selection
(Change `[ ]` to `[x]` for the courses you plan to use this final project for.)

- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [ ] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [ ] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [ ] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### 3D Scene Node Graph Editor with Transformations

#### Motivation
Building a 3D scene is a lot easier when you can see the node hierarchy and tweak transforms interactively. This project is a small desktop editor that does exactly that — no full game engine, just a clean scene graph you can poke around in and a live 3D viewport to see what's happening. It's also a good excuse to practice C# OOP: subclassing, interfaces, and custom JSON serialisation for 3D math types.

#### Use Case Scenarios
- Someone learning 3D can add nodes, drag transforms around, and immediately see how a child's world position changes when the parent moves.
- Someone sketching a quick scene layout can drop in lights, cameras, and mesh placeholders, tweak their properties, and export the result to JSON.

#### Main Features
- **Scene hierarchy**: a tree of typed nodes — root, Camera, Light, and Mesh — connected by parent-child links.
- **Add nodes**: add a Camera, Light, or Mesh as a child of the currently selected node (falls back to root when nothing is selected).
- **Select nodes**: click any node in the hierarchy tree to select it.
- **Rename nodes**: edit the Tag field for any node in the Inspector.
- **Edit transforms**: drag Position (Vec3), Rotation (Quaternion XYZW, auto-normalised), and Scale (Vec3) for any node.
- **Edit type-specific properties**:
  - *Camera*: projection type (Perspective / Orthographic), field of view (radians), near/far clipping planes, aspect ratio.
  - *Light*: type (Point / Directional / Spot), intensity, decay, RGB colour, and cutoff angle (Spot only).
  - *Mesh*: mesh asset path string (metadata only; rendered as a placeholder cube at runtime).
- **Delete node**: remove the selected non-root node from its parent.
- **Reset scene**: rebuild the default scene (one Camera, one Mesh, one Point Light).
- **3D viewport**: live-updating view rendered into an ImGui image with orbit/pan/zoom camera control.

#### UI/UX
Desktop GUI using MonoGame (OpenGL) with an ImGui overlay. The window opens at 1440×900 with three panels:

1. **Scene panel** — collapsible node tree. Each node shows a type prefix (`[CAM]`, `[LIT]`, `[MSH]`, `[NODE]`) and its tag. Buttons at the top add nodes or reset the whole scene.
2. **Inspector panel** — editable fields for whatever is selected: tag, transform (position/rotation/scale), and type-specific stuff (projection type, FOV, light colour, etc.). Non-root nodes also get a Delete button.
3. **Viewport panel** — resizable live 3D view. Right-drag to orbit, middle-drag to pan, scroll to zoom.

The viewport renders:
- A 20×20 reference grid with a red X-axis line and a blue Z-axis line.
- An axis-cross gizmo at every node's world position — orange and larger for the selected node, yellow for cameras, the light's own colour for lights, grey for everything else.
- Mesh nodes as plain white unit cubes (no lighting or textures).

#### Persistence
The scene graph can be saved and loaded as JSON via serialization library (System.Text.Json).

The editor UI doesn't have a load/save dialog yet — serialisation is there as a library feature.

#### Libraries and Technologies
- **MonoGame** (DesktopGL): windowing, graphics device, vertex/index buffers, render targets.
- **ImGui.NET / MonoGame.ImGuiNet**: immediate-mode UI panels and the viewport image widget.
- **System.Text.Json**: built-in JSON serialization with custom converters and polymorphic type handling.
- **xUnit**: unit test framework.

#### Testing
30 xUnit tests in `SceneOrchestrator.Tests`.

Run all tests with:
```
dotnet test SceneOrchestrator.slnx
```
Run the application with:
```
dotnet run --project SceneOrchestrator.App
```
