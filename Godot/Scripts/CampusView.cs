using Godot;
using KoSch.SchoolTycoon.Core;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CampusView : Node3D
{
    private RektorApp app;
    public Camera3D Camera { get; private set; }
    private Node3D buildings, community, ghost;
    private DirectionalLight3D sun;
    private readonly Dictionary<int, Node3D> roomNodes = new();
    private readonly Dictionary<string, Mesh> meshes = new();
    private readonly Dictionary<string, StandardMaterial3D> materials = new();
    private readonly List<CampusWalker> walkers = new();
    private readonly Dictionary<int, Vector2> fingers = new();
    private Vector2 press, last;
    private bool dragging;
    private float travel, yaw = -.65f, distance = 25, pinch;
    private Vector3 focus = new(15, 0, 11);
    private Cell ghostCell = new(-999, -999);

    public void Initialize(RektorApp owner)
    {
        app = owner;
        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new Color("d7e6dd"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new Color("f5e8cf"),
            AmbientLightEnergy = .4f, ReflectedLightSource = Godot.Environment.ReflectionSource.Disabled
        };
        AddChild(new WorldEnvironment { Environment = environment });
        sun = new DirectionalLight3D { RotationDegrees = new Vector3(-48, -28, 0), LightColor = new Color("fff0d6"), LightEnergy = .75f, ShadowEnabled = true };
        AddChild(sun);
        Camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = distance, Far = 180, Current = true };
        AddChild(Camera);
        var landscape = new Node3D { Name = "Neighbourhood" }; AddChild(landscape); Draw(landscape, CampusDesign.Landscape());
        Rebuild(); Home();
    }
    public void Home() { focus = new Vector3(15, app.Floor * 3, 11); yaw = -.65f; distance = 25; PositionCamera(); }
    public void Orbit() { yaw += MathF.PI / 2; PositionCamera(); }
    public void Zoom(float n) { distance = Math.Clamp(distance + n, 9, 55); PositionCamera(); }
    private void PositionCamera()
    {
        Camera.Size = distance;
        Camera.Position = focus + new Vector3(MathF.Cos(yaw) * 28, 26, MathF.Sin(yaw) * 28);
        Camera.LookAt(focus, Vector3.Up);
    }
    public void Rebuild()
    {
        ClearGhost();
        foreach (var old in new[] { buildings, community }) if (old != null) { RemoveChild(old); old.QueueFree(); }
        buildings = new Node3D { Name = "Rooms" }; community = new Node3D { Name = "People" };
        AddChild(buildings); AddChild(community); roomNodes.Clear(); walkers.Clear();
        var reachable = CampusGrid.Reachable(app.State);
        foreach (Room r in app.State.Rooms)
        {
            var root = new Node3D { Name = "Room" + r.Id, Position = new Vector3(r.X, r.Floor * 3, r.Y) };
            buildings.AddChild(root); roomNodes.Add(r.Id, root);
            int side = 2; float at = .5f;
            if (CampusGrid.TryDoor(r, reachable, out Cell door, out Cell inside))
            {
                if (door.Y >= r.Y + r.Height) { side = 0; at = inside.X - r.X + .5f; }
                else if (door.X < r.X) { side = 1; at = inside.Y - r.Y + .5f; }
                else if (door.X >= r.X + r.Width) { side = 3; at = inside.Y - r.Y + .5f; }
                else { side = 2; at = inside.X - r.X + .5f; }
            }
            bool rises = app.State.Rooms.Any(upper => upper.Kind == RoomKind.Staircase && upper.Floor == r.Floor + 1 && upper.X == r.X && upper.Y == r.Y);
            Draw(root, CampusDesign.Room(r, side, at, rises));
            if (!CampusGrid.Walkway(r))
            {
                var group = app.State.Classes.FirstOrDefault(c => c.RoomId == r.Id);
                var label = new Label3D { Name = "Label", Text = group == null ? app.RoomName(r.Kind) : group.Name + " · " + app.Sim.ClassSize(group) + "/" + app.Sim.RoomSeats(r),
                    Position = new Vector3(r.Width / 2f, 3.05f, r.Height / 2f), FontSize = 40, PixelSize = .006f,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Modulate = new Color("355a50"), OutlineModulate = new Color("faf5e6"), OutlineSize = 8, NoDepthTest = false };
                root.AddChild(label);
            }
        }
        int limit = app.Settings.Graphics == 0 ? 12 : app.Settings.Graphics == 1 ? 24 : 40;
        foreach (var p in app.State.Pupils.Where(p => !p.Absent).Take(limit)) AddPerson(p.Id, null, p);
        foreach (var e in app.State.Staff.Take(16)) AddPerson(e.Id + 100, e, null);
        if (app.State.HasDirector) AddPerson(201, new Employee { Name = app.State.Director.Name, Role = StaffRole.Counselor }, null, true);
        UpdateVisibility();
    }
    private void AddPerson(int id, Employee employee, Pupil pupil, bool director = false)
    {
        bool adult = employee != null;
        var walker = new CampusWalker { Name = "Person" + id, Position = new Vector3(14.5f, .16f, 8.5f) };
        community.AddChild(walker);
        var body = new Node3D { Scale = Vector3.One * (adult ? .93f : .76f) }; walker.AddChild(body);
        var bones = new Node3D[4];
        Vector3[] pivots = { new(-.215f, .8f, 0), new(.215f, .8f, 0), new(-.084f, .42f, 0), new(.084f, .42f, 0) };
        for (int i = 0; i < 4; i++) { bones[i] = new Node3D { Position = pivots[i] }; body.AddChild(bones[i]); }
        string hair = director ? app.State.Director.Hair switch { "Black" => "403b37", "Blonde" => "dfc489", "Red" => "b78966", "Silver" => "c8d4d3", _ => "796047" } : id % 2 == 0 ? "796047" : "bfa078";
        var parts = CampusDesign.Person(id, adult, hair, director ? app.State.Director.Gender : "Diverse");
        Draw(body, parts.Where(p => p.Bone == 0));
        for (int i = 0; i < 4; i++) Draw(bones[i], parts.Where(p => p.Bone == i + 1), -pivots[i]);
        walker.Initialize(app, id, employee, pupil, bones); walkers.Add(walker);
    }
    public void UpdateVisibility()
    {
        if (app == null) return;
        sun.ShadowEnabled = app.Settings.Graphics > 0;
        GetViewport().Msaa3D = app.Settings.Graphics == 0 ? Viewport.Msaa.Disabled : app.Settings.Graphics == 1 ? Viewport.Msaa.Msaa2X : Viewport.Msaa.Msaa4X;
        foreach (var r in app.State.Rooms)
        {
            if (!roomNodes.TryGetValue(r.Id, out var root)) continue;
            root.Visible = r.Floor <= app.Floor;
            var label = root.GetNodeOrNull<Label3D>("Label");
            if (label != null) label.Visible = app.Settings.Labels && r.Floor == app.Floor;
        }
        foreach (var walker in walkers) walker.Visible = walker.Position.Y < (app.Floor + 1) * 3;
        focus.Y = app.Floor * 3; PositionCamera();
    }
    private StandardMaterial3D Material(VisualPart p)
    {
        string key = p.Color + ":" + p.Surface;
        if (materials.TryGetValue(key, out var mat)) return mat;
        mat = new StandardMaterial3D { AlbedoColor = new Color(p.Color), Roughness = p.Surface == VisualSurface.Glass ? .15f : .75f, Metallic = p.Surface == VisualSurface.Metal ? .3f : 0 };
        if (p.Surface == VisualSurface.Glass) { mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha; var c = mat.AlbedoColor; c.A = .5f; mat.AlbedoColor = c; }
        if (p.Surface == VisualSurface.Screen) { mat.EmissionEnabled = true; mat.Emission = new Color(p.Color); mat.EmissionEnergyMultiplier = .2f; }
        materials.Add(key, mat); return mat;
    }
    private Mesh Mesh(VisualPart p)
    {
        string key = p.Shape == VisualShape.Box ? FormattableString.Invariant($"box:{p.W}:{p.H}:{p.D}") : p.Shape.ToString();
        if (meshes.TryGetValue(key, out var m)) return m;
        m = p.Shape switch
        {
            VisualShape.Box => RoundedBox(new Vector3(p.W, p.H, p.D)),
            VisualShape.Sphere => new SphereMesh { Radius = .5f, Height = 1, RadialSegments = 24, Rings = 12 },
            VisualShape.Cylinder => new CylinderMesh { TopRadius = .5f, BottomRadius = .5f, Height = 1, RadialSegments = 20 },
            _ => new CapsuleMesh { Radius = .5f, Height = 2, RadialSegments = 20, Rings = 8 }
        };
        meshes.Add(key, m); return m;
    }
    private void Draw(Node3D parent, IEnumerable<VisualPart> parts, Vector3 offset = default)
    {
        var batches = new Dictionary<(Mesh, StandardMaterial3D), List<Transform3D>>();
        foreach (var p in parts)
        {
            var key = (Mesh(p), Material(p));
            if (!batches.TryGetValue(key, out var list)) batches.Add(key, list = new());
            var basis = new Basis(Vector3.Up, Mathf.DegToRad(p.Rotation));
            if (p.Shape != VisualShape.Box) basis = basis.Scaled(new Vector3(p.W, p.Shape == VisualShape.Capsule ? p.H / 2 : p.H, p.D));
            list.Add(new Transform3D(basis, new Vector3(p.X, p.Y, p.Z) + offset));
        }
        foreach (var pair in batches)
        {
            var multi = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = pair.Key.Item1, InstanceCount = pair.Value.Count };
            for (int i = 0; i < pair.Value.Count; i++) multi.SetInstanceTransform(i, pair.Value[i]);
            parent.AddChild(new MultiMeshInstance3D { Multimesh = multi, MaterialOverride = pair.Key.Item2 });
        }
    }
    private static ArrayMesh RoundedBox(Vector3 size)
    {
        float radius = Math.Min(.055f, Math.Min(size.X, Math.Min(size.Y, size.Z)) * .21f);
        Vector3 half = size * .5f, inner = half - Vector3.One * radius;
        var surface = new SurfaceTool(); surface.Begin(Godot.Mesh.PrimitiveType.Triangles);
        Vector3[] normal = { Vector3.Right, Vector3.Left, Vector3.Up, Vector3.Down, Vector3.Back, Vector3.Forward };
        Vector3[] us = { Vector3.Forward, Vector3.Back, Vector3.Right, Vector3.Right, Vector3.Right, Vector3.Left };
        Vector3[] vs = { Vector3.Up, Vector3.Up, Vector3.Forward, Vector3.Back, Vector3.Up, Vector3.Up };
        for (int f = 0; f < 6; f++)
        {
            Vector3 face = normal[f], u = us[f], v = vs[f];
            float a = half.Dot(u.Abs()), b = half.Dot(v.Abs()), c = half.Dot(face.Abs());
            float[] xs = { -a, -a + radius, a - radius, a }, ys = { -b, -b + radius, b - radius, b };
            var points = new Vector3[16]; var norms = new Vector3[16];
            for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++)
            {
                Vector3 p = face * c + u * xs[x] + v * ys[y];
                Vector3 center = p.Clamp(-inner, inner); Vector3 n = (p - center).Normalized();
                points[y * 4 + x] = center + n * radius; norms[y * 4 + x] = n;
            }
            for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++)
            {
                int i = y * 4 + x;
                foreach (int index in new[] { i, i + 4, i + 1, i + 1, i + 4, i + 5 }) { surface.SetNormal(norms[index]); surface.AddVertex(points[index]); }
            }
        }
        return surface.Commit();
    }
    public Cell? Pick(Vector2 screen)
    {
        var plane = new Plane(Vector3.Up, app.Floor * 3 + .15f);
        Vector3? p = plane.IntersectsRay(Camera.ProjectRayOrigin(screen), Camera.ProjectRayNormal(screen));
        if (!p.HasValue) return null;
        return new Cell(Mathf.FloorToInt(p.Value.X), Mathf.FloorToInt(p.Value.Z), app.Floor);
    }
    public void ClearGhost()
    {
        if (ghost != null) { RemoveChild(ghost); ghost.QueueFree(); ghost = null; }
        ghostCell = new Cell(-999, -999);
    }
    private void ShowGhost(Vector2 position)
    {
        Cell? cell = Pick(position);
        if (!cell.HasValue || !app.BuildKind.HasValue) { ClearGhost(); return; }
        if (ghostCell.Equals(cell.Value)) return;
        ClearGhost(); ghostCell = cell.Value;
        var spec = Catalog.Get(app.BuildKind.Value); int w = app.Rotated ? spec.Height : spec.Width, h = app.Rotated ? spec.Width : spec.Height;
        bool valid = app.Sim.CanBuild(spec.Kind, ghostCell.X, ghostCell.Y, app.Rotated, app.Floor).Success;
        ghost = new Node3D { Position = new Vector3(ghostCell.X, app.Floor * 3 + .25f, ghostCell.Y) }; AddChild(ghost);
        ghost.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = new Vector3(w, .04f, h) }, Position = new Vector3(w / 2f, 0, h / 2f),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = valid ? new Color(.35f, .7f, .57f, .6f) : new Color(.85f, .4f, .35f, .6f), Transparency = BaseMaterial3D.TransparencyEnum.Alpha } });
    }
    private void Pan(Vector2 delta)
    {
        Vector3 right = Camera.GlobalBasis.X, forward = Camera.GlobalBasis.Y; forward.Y = 0; right.Y = 0;
        focus -= (right.Normalized() * delta.X + forward.Normalized() * delta.Y) * distance / 900 * app.Settings.CameraSpeed;
        focus.X = Math.Clamp(focus.X, 0, 32); focus.Z = Math.Clamp(focus.Z, 0, 24); PositionCamera();
    }
    public override void _UnhandledInput(InputEvent e)
    {
        if (app.Menu || app.UI.ModalOpen) return;
        if (e is InputEventMouseButton mouse && !OS.HasFeature("android"))
        {
            if (mouse.ButtonIndex == MouseButton.WheelUp && mouse.Pressed) Zoom(-1.5f);
            if (mouse.ButtonIndex == MouseButton.WheelDown && mouse.Pressed) Zoom(1.5f);
            if (mouse.ButtonIndex == MouseButton.Left || mouse.ButtonIndex == MouseButton.Right)
            {
                if (mouse.Pressed) { dragging = true; press = last = mouse.Position; travel = 0; }
                else if (dragging) { dragging = false; if (travel < 12 && mouse.ButtonIndex == MouseButton.Left) { var c = Pick(mouse.Position); if (c.HasValue) app.BuildAt(c.Value); } }
            }
        }
        if (e is InputEventMouseMotion motion && !OS.HasFeature("android"))
        {
            ShowGhost(motion.Position);
            if (dragging) { travel += motion.Relative.Length(); if (!app.BuildKind.HasValue || motion.ButtonMask == MouseButtonMask.Right) Pan(motion.Relative); }
        }
        if (e is InputEventScreenTouch touch)
        {
            if (touch.Pressed) { fingers[touch.Index] = touch.Position; if (fingers.Count == 1) { press = last = touch.Position; travel = 0; } else travel = 100; }
            else
            {
                bool tap = fingers.Count == 1 && fingers.ContainsKey(touch.Index) && travel < 12;
                fingers.Remove(touch.Index); pinch = 0;
                if (tap) { var c = Pick(touch.Position); if (c.HasValue) app.BuildAt(c.Value); }
            }
        }
        if (e is InputEventScreenDrag drag && fingers.ContainsKey(drag.Index))
        {
            fingers[drag.Index] = drag.Position; travel += drag.Relative.Length();
            if (fingers.Count >= 2) { var ps = fingers.Values.Take(2).ToArray(); float d = ps[0].DistanceTo(ps[1]); if (pinch > 0) Zoom((pinch - d) * .035f); pinch = d; }
            else { Pan(drag.Relative); ShowGhost(drag.Position); }
        }
        if (e is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.Space) app.Paused = !app.Paused;
            if (key.Keycode == Key.R) { app.Rotated = !app.Rotated; ClearGhost(); app.UI.RefreshStats(); }
            if (key.Keycode == Key.Home) Home();
        }
    }
}

public partial class CampusWalker : Node3D
{
    private RektorApp app;
    private Employee employee;
    private Pupil pupil;
    private int id, destination = -1, lastSlot = -99;
    private readonly Queue<Vector3> route = new();
    private Cell corridor = CampusGrid.Entrance;
    private Node3D[] bones;
    private float phase;
    public void Initialize(RektorApp owner, int number, Employee e, Pupil p, Node3D[] limbs)
    {
        app = owner; id = number; employee = e; pupil = p; bones = limbs;
        int roomId = pupil?.ClassRoomId ?? app.State.Classes.FirstOrDefault(c => c.TeacherId == employee?.Id)?.RoomId ?? 1;
        var home = app.State.Rooms.FirstOrDefault(r => r.Id == roomId);
        if (home != null)
        {
            Position = new Vector3(home.X + .5f + id % 3 * .65f, home.Floor * 3 + .16f, home.Y + .5f + id / 3 % 3 * .55f);
            if (CampusGrid.TryDoor(home, CampusGrid.Reachable(app.State), out var door, out _)) corridor = door;
        }
    }
    public override void _Process(double delta)
    {
        if (app.Menu) return;
        int slot = SchoolSimulation.LessonSlot(app.State.ClockMinute);
        if (slot != lastSlot || destination < 0) { lastSlot = slot; Plan(); }
        bool moving = false;
        if (route.Count > 0 && !app.Paused)
        {
            Vector3 target = route.Peek(), diff = target - Position;
            Position = Position.MoveToward(target, (float)delta * 1.6f * Math.Min(2, app.Speed));
            if (diff.Length() > .03f) { Rotation = new Vector3(0, MathF.Atan2(-diff.X, -diff.Z), 0); moving = true; }
            if (Position.DistanceTo(target) < .025f) route.Dequeue();
        }
        phase += (float)delta * 7;
        for (int i = 0; i < bones.Length; i++) bones[i].Rotation = new Vector3(moving && !app.Settings.ReducedMotion ? MathF.Sin(phase + i % 2 * MathF.PI) * .3f : 0, 0, 0);
        Visible = Position.Y < (app.Floor + 1) * 3;
    }
    private void Plan()
    {
        Room target = app.Sim.Destination(pupil, employee, app.State.ClockMinute, id);
        if (target == null || target.Id == destination) return;
        if (!CampusGrid.TryDoor(target, CampusGrid.Reachable(app.State), out Cell door, out Cell inside)) return;
        var path = CampusGrid.Path(app.State, corridor, door); if (path.Count == 0) return;
        route.Clear(); route.Enqueue(new Vector3(corridor.X + .5f, corridor.Floor * 3 + .16f, corridor.Y + .5f));
        foreach (Cell c in path) route.Enqueue(new Vector3(c.X + .5f, c.Floor * 3 + .16f, c.Y + .5f));
        route.Enqueue(new Vector3(inside.X + .5f, inside.Floor * 3 + .16f, inside.Y + .5f));
        route.Enqueue(new Vector3(target.X + .65f + id % 3 * .3f, target.Floor * 3 + .16f, target.Y + .65f + id % 2 * .4f));
        corridor = door; destination = target.Id;
    }
}
