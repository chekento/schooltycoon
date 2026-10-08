using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon
{
    /// <summary>Original procedural cutaway campus. Shared materials, no downloaded art assets.</summary>
    public sealed class CampusRenderer : MonoBehaviour
    {
        private SchoolApp app;
        private Transform buildings, people, ghost;
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private readonly List<CampusPerson> agents = new List<CampusPerson>();
        private Cell ghostCell = new Cell(-1, -1);
        private RoomKind? ghostKind;
        private bool ghostRotated;
        public void Initialize(SchoolApp owner)
        {
            app = owner;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ColorHex("D8EDFD");
            RenderSettings.ambientEquatorColor = ColorHex("B8C9CF");
            RenderSettings.ambientGroundColor = ColorHex("8EACA0");
            RenderSettings.fog = false;
            var sun = new GameObject("Afternoon sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.color = ColorHex("FFF3DB"); sun.intensity = 1.05f;
            sun.transform.rotation = Quaternion.Euler(48, -25, 0); sun.shadows = LightShadows.Soft;
            QualitySettings.shadowDistance = 75; QualitySettings.shadows = ShadowQuality.All; QualitySettings.antiAliasing = 4;
            Environment(); Rebuild();
        }
        public static Color ColorHex(string hex) { Color c; ColorUtility.TryParseHtmlString("#" + hex, out c); return c; }
        private Material Material(string hex)
        {
            Material material;
            if (materials.TryGetValue(hex, out material)) return material;
            Shader shader = Resources.Load<Shader>("CampusSurface");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            material = new Material(shader) { color = ColorHex(hex) };
            material.SetFloat("_Glossiness", .08f); material.enableInstancing = true;
            materials.Add(hex, material); return material;
        }
        private GameObject Shape(Transform parent, string name, PrimitiveType kind, Vector3 position, Vector3 scale, string hex)
        {
            GameObject go = GameObject.CreatePrimitive(kind); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Material(hex);
            Collider collider = go.GetComponent<Collider>(); if (collider != null) Destroy(collider);
            return go;
        }
        private GameObject Box(Transform p, string name, float x, float y, float z, float w, float h, float d, string hex)
        { return Shape(p, name, PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(w, h, d), hex); }
        private Transform Group(string name, Transform parent)
        { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private void Environment()
        {
            Transform env = Group("Neighbourhood and meadow", transform);
            Box(env, "Floating meadow base", 16, -.42f, 12, 35, .7f, 27, "648C6A");
            Box(env, "Meadow", 16, -.08f, 12, 34, .08f, 26, "A7C98C");
            for (int x = 0; x < CampusGrid.Width; x++) for (int z = 0; z < CampusGrid.Height; z++)
                Box(env, "Land tile", x + .5f, -.025f, z + .5f, .98f, .045f, .98f, (x + z) % 2 == 0 ? "A9CC8C" : "A5C888");
            Box(env, "Street", 16, .01f, -1.35f, 35, .06f, 1.9f, "69818C");
            for (int i = 0; i < 18; i++) Box(env, "Road stripe", i * 2, .055f, -1.35f, .85f, .02f, .06f, "F5EECF");
            Box(env, "Entrance walkway", 14.5f, .012f, 4, 1.65f, .07f, 8, "C6D5D4");
            for (int i = 0; i < 34; i++)
            {
                float x = i < 17 ? .8f + i * 1.85f : (i % 2 == 0 ? .45f : 31.5f);
                float z = i < 17 ? 22.9f : 2 + (i - 17) * 1.22f;
                Tree(env, x, z, .7f + (i % 4) * .12f);
            }
            for (int i = 0; i < 18; i++)
            {
                float x = 2 + (i * 7) % 27, z = 2 + (i * 11) % 19;
                if (x >= 6 && x < 26 && z >= 5 && z < 21) continue;
                Shape(env, "Shrub", PrimitiveType.Sphere, new Vector3(x, .22f, z), new Vector3(.9f, .55f, .65f), "79AC69");
                for (int j = 0; j < 3; j++) Shape(env, "Flowers", PrimitiveType.Sphere, new Vector3(x + j * .19f - .15f, .5f, z + .1f), Vector3.one * .15f, j % 2 == 0 ? "F8D676" : "ECA1BA");
            }
            Fence(env, 5.65f, 4.7f, 20.7f, true);
            Fence(env, 26.3f, 4.7f, 20.7f, true);
            Fence(env, 5.65f, 21.3f, 26.3f, false);
            Box(env, "School sign post", 12.8f, .9f, 2.5f, .13f, 1.8f, .13f, "81958F");
            Box(env, "School sign", 12.8f, 1.45f, 2.5f, 2, .75f, .16f, "265A65");
            Label(env, "KOSCH\nCAMPUS", new Vector3(12.8f, 1.46f, 2.37f), .12f, "FFFFFF", false);
            Bench(env, 13.1f, 4.6f); Bench(env, 16.5f, 4.6f);
            // A pair of cheerful street-side buildings gives the campus a sense of place.
            for (int i = 0; i < 3; i++)
            {
                float x = 3 + i * 11;
                Box(env, "Neighbouring house", x, 1.1f, -4, 4, 2.2f, 2.6f, i == 0 ? "E9BA98" : i == 1 ? "9DC5CF" : "C3A9D5");
                Box(env, "Flat roof", x, 2.3f, -4, 4.2f, .22f, 2.8f, "5F788A");
                for (int j = 0; j < 3; j++) Box(env, "Window", x - 1.2f + j * 1.2f, 1.25f, -2.68f, .65f, .9f, .07f, "D8EFF5");
            }
        }
        private void Fence(Transform p, float x, float from, float to, bool vertical)
        {
            for (float a = from; a <= to; a += 1.5f)
                Box(p, "Fence post", vertical ? x : a, .36f, vertical ? a : x, .12f, .72f, .12f, "EAE3CC");
            Box(p, "Fence rail", vertical ? x : (from + to) / 2, .35f, vertical ? (from + to) / 2 : x, vertical ? .06f : to - from, .1f, vertical ? to - from : .06f, "EAE3CC");
        }
        private void Tree(Transform p, float x, float z, float size)
        {
            Shape(p, "Tree trunk", PrimitiveType.Cylinder, new Vector3(x, .55f * size, z), new Vector3(.18f * size, .55f * size, .18f * size), "956F4F");
            Shape(p, "Tree crown", PrimitiveType.Sphere, new Vector3(x, 1.6f * size, z), new Vector3(1.45f, 1.65f, 1.35f) * size, "679E65");
            Shape(p, "Tree highlight", PrimitiveType.Sphere, new Vector3(x + .25f * size, 1.9f * size, z - .14f), new Vector3(.9f, 1, .9f) * size, "82B774");
        }
        private void Bench(Transform p, float x, float z)
        {
            Box(p, "Bench seat", x, .35f, z, 1.1f, .12f, .38f, "B99263");
            Box(p, "Bench back", x, .6f, z + .16f, 1.1f, .45f, .09f, "C6A16D");
            for (int i = -1; i <= 1; i += 2) Box(p, "Bench leg", x + i * .4f, .15f, z, .06f, .3f, .3f, "597783");
        }
        public void Rebuild()
        {
            ClearGhost();
            if (buildings != null) { buildings.gameObject.SetActive(false); Destroy(buildings.gameObject); }
            if (people != null) { people.gameObject.SetActive(false); Destroy(people.gameObject); }
            buildings = Group("Rooms", transform); people = Group("People", transform); agents.Clear();
            foreach (Room room in app.State.Rooms) RoomVisual(room);
            int studentVisuals = Mathf.Min(app.State.Students, 64);
            for (int i = 0; i < studentVisuals; i++) Person(i, false, null);
            for (int i = 0; i < Mathf.Min(app.State.Staff.Count, 24); i++) Person(i + 100, true, app.State.Staff[i]);
            if (app.State.HasDirector) Person(201, true, new Employee { Name = app.State.Director.Name, Role = StaffRole.Counselor }, true);
        }
        private void RoomVisual(Room room)
        {
            var spec = Catalog.Get(room.Kind);
            Transform p = Group(spec.NameEn + " " + room.Id, buildings);
            p.localPosition = new Vector3(room.X, 0, room.Y);
            float w = room.Width, h = room.Height;
            Box(p, "Floor edge", w / 2, .055f, h / 2, w - .02f, .11f, h - .02f, "849CA3");
            Box(p, "Floor", w / 2, .12f, h / 2, w - .1f, .04f, h - .1f, spec.Hex);
            if (room.Kind == RoomKind.Corridor)
            {
                for (int x = 0; x < room.Width; x++) for (int z = 0; z < room.Height; z++)
                    Box(p, "Corridor inlay", x + .5f, .148f, z + .5f, .7f, .01f, .7f, "CFDBE1");
                return;
            }
            if (room.Kind == RoomKind.Garden)
            {
                Box(p, "Garden path", w / 2, .16f, h / 2, .65f, .03f, h - .2f, "D8D2AB");
                Tree(p, .65f, h - .7f, .62f); Bench(p, w - 1, .75f);
                for (int i = 0; i < 4; i++)
                {
                    Box(p, "Raised bed", .75f, .27f, .5f + i * .46f, .72f, .22f, .37f, "A98760");
                    for (int j = 0; j < 3; j++) Shape(p, "Vegetables", PrimitiveType.Sphere, new Vector3(.5f + j * .23f, .43f, .5f + i * .46f), Vector3.one * .23f, "609B61");
                }
            }
            else
            {
                // Two high walls and two low walls: an intentional dollhouse cutaway.
                Box(p, "North wall", w / 2, .72f, h - .06f, w, 1.3f, .12f, "F7EEDC");
                Box(p, "West wall", .06f, .72f, h / 2, .12f, 1.3f, h, "E4E6DA");
                Box(p, "South trim", w / 2, .24f, .06f, w, .22f, .12f, "F7EEDC");
                Box(p, "East trim", w - .06f, .24f, h / 2, .12f, .22f, h, "F7EEDC");
                for (float x = .7f; x < w - .4f; x += 1.2f)
                    Box(p, "Window", x, .93f, h - .135f, .7f, .52f, .025f, "AFD9E4");
                Furnish(p, room.Kind, w, h);
            }
            var reachable = CampusGrid.Reachable(CampusGrid.Corridors(app.State)); Cell door, inside;
            if (CampusGrid.TryDoor(room, reachable, out door, out inside))
            {
                Vector3 position = new Vector3((door.X + inside.X) * .5f + .5f - room.X, .17f, (door.Y + inside.Y) * .5f + .5f - room.Y);
                Box(p, "Door mat", position.x, position.y, position.z, door.X != inside.X ? .35f : .7f, .07f, door.X != inside.X ? .7f : .35f, "6FA9A5");
            }
            Label(p, app.RoomName(room.Kind), new Vector3(w / 2, 1.8f, h / 2), .115f, "234B5B", true);
        }
        private void Desk(Transform p, float x, float z, string top = "D8B67A")
        {
            Box(p, "Desk top", x, .57f, z, .65f, .09f, .46f, top);
            for (int i = -1; i <= 1; i += 2) Box(p, "Desk leg", x + i * .24f, .33f, z, .055f, .47f, .31f, "597987");
            Box(p, "Chair seat", x, .35f, z - .39f, .34f, .07f, .33f, "609FC0");
            Box(p, "Chair back", x, .54f, z - .52f, .34f, .37f, .055f, "609FC0");
            Box(p, "Notebook", x, .63f, z, .19f, .015f, .22f, "F5F1D8");
        }
        private void Furnish(Transform p, RoomKind kind, float w, float h)
        {
            switch (kind)
            {
                case RoomKind.Classroom:
                    for (int i = 0; i < 3; i++) for (int j = 0; j < 2; j++) Desk(p, .85f + i * (w - 1.3f) / 3, .85f + j * .95f);
                    Box(p, "Chalkboard", w / 2, 1.06f, h - .16f, 1.9f, .55f, .04f, "396D60");
                    for (int i = 0; i < 4; i++) Box(p, "Chalk writing", w / 2 - .6f + i * .35f, 1.1f, h - .19f, .18f, .03f, .01f, "EBF3DA");
                    break;
                case RoomKind.Toilet:
                    for (int i = 0; i < 2; i++)
                    {
                        Shape(p, "Toilet bowl", PrimitiveType.Cylinder, new Vector3(.5f + i * .9f, .33f, h - .6f), new Vector3(.43f, .18f, .52f), "F6FAF3");
                        Box(p, "Cistern", .5f + i * .9f, .65f, h - .32f, .38f, .46f, .23f, "F6FAF3");
                    }
                    Box(p, "Washbasin", .5f, .63f, .42f, .6f, .19f, .4f, "E9F5ED");
                    Box(p, "Mirror", .14f, .96f, .47f, .025f, .35f, .4f, "A8D7E3");
                    break;
                case RoomKind.Staffroom:
                    Desk(p, 1.1f, .95f, "B9A1D5");
                    Box(p, "Sofa", w - .56f, .45f, h / 2, .56f, .56f, 1.15f, "937BB6");
                    Box(p, "Coffee cabinet", .55f, .55f, h - .37f, .7f, .9f, .42f, "B3977B");
                    Box(p, "Coffee maker", .55f, 1.08f, h - .37f, .24f, .25f, .25f, "405B67");
                    break;
                case RoomKind.Canteen:
                    for (int i = 0; i < 2; i++) for (int j = 0; j < 2; j++)
                    {
                        float x = 1 + i * 1.8f, z = .8f + j * 1.05f;
                        Desk(p, x, z, "F1BC8A");
                        Shape(p, "Plate", PrimitiveType.Cylinder, new Vector3(x, .65f, z), new Vector3(.2f, .01f, .2f), "E8EEE7");
                    }
                    Box(p, "Serving counter", w / 2, .65f, h - .42f, w - .5f, .95f, .5f, "D2856E");
                    break;
                case RoomKind.Library:
                    for (int i = 0; i < 3; i++) Shelf(p, .5f + i * 1.2f, h - .42f);
                    Desk(p, 1.1f, 1); Desk(p, w - 1.1f, 1);
                    break;
                case RoomKind.ScienceLab:
                    for (int i = 0; i < 3; i++)
                    {
                        float x = .7f + i * 1.1f; Desk(p, x, 1.4f, "BCD8DC");
                        Shape(p, "Flask", PrimitiveType.Sphere, new Vector3(x, .76f, 1.4f), new Vector3(.17f, .24f, .17f), "52BEB1");
                        Shape(p, "Flask neck", PrimitiveType.Cylinder, new Vector3(x, .91f, 1.4f), new Vector3(.05f, .09f, .05f), "80D9D4");
                    }
                    Shelf(p, 1.2f, h - .4f); break;
                case RoomKind.ArtRoom:
                    for (int i = 0; i < 3; i++)
                    {
                        float x = .8f + i * 1.15f;
                        Box(p, "Easel", x, .65f, 1.5f, .06f, 1, .07f, "B88F64");
                        Box(p, "Canvas", x, .95f, 1.47f, .6f, .63f, .07f, "FFF4DE");
                        Shape(p, "Painting", PrimitiveType.Sphere, new Vector3(x, .95f, 1.41f), new Vector3(.34f, .33f, .025f), i == 0 ? "D879A7" : i == 1 ? "69B4C8" : "F3CB65");
                    }
                    break;
                case RoomKind.Gym:
                    Box(p, "Court", w / 2, .155f, h / 2, w - .75f, .01f, h - .7f, "DDB683");
                    Box(p, "Court centre", w / 2, .17f, h / 2, .05f, .01f, h - .7f, "FFF2D2");
                    Shape(p, "Basketball", PrimitiveType.Sphere, new Vector3(w / 2 + .5f, .34f, h / 2), Vector3.one * .35f, "D88D52");
                    Box(p, "Basket board", w / 2, 1.15f, h - .22f, .7f, .5f, .04f, "E9EEDD");
                    break;
            }
            Shape(p, "Potted plant", PrimitiveType.Cylinder, new Vector3(.3f, .28f, .3f), new Vector3(.24f, .14f, .24f), "BC8D6B");
            Shape(p, "Plant leaves", PrimitiveType.Sphere, new Vector3(.3f, .52f, .3f), new Vector3(.35f, .45f, .32f), "75A86D");
        }
        private void Shelf(Transform p, float x, float z)
        {
            Box(p, "Bookcase", x, .73f, z, .9f, 1.15f, .3f, "B58C68");
            for (int row = 0; row < 3; row++) for (int j = 0; j < 5; j++)
                Box(p, "Book", x - .32f + j * .16f, .4f + row * .33f, z - .17f, .11f, .22f, .08f, j % 3 == 0 ? "6AA9BD" : j % 3 == 1 ? "DB938B" : "E7CC7A");
        }
        private void Label(Transform p, string text, Vector3 pos, float size, string hex, bool billboard)
        {
            Transform t = Group("Label", p); t.localPosition = pos;
            TextMesh mesh = t.gameObject.AddComponent<TextMesh>(); mesh.text = text; mesh.characterSize = size;
            mesh.fontSize = 48; mesh.color = ColorHex(hex); mesh.anchor = TextAnchor.MiddleCenter;
            if (billboard) t.gameObject.AddComponent<CampusBillboard>();
        }
        private void Person(int index, bool adult, Employee employee, bool director = false)
        {
            Transform p = Group(adult ? employee.Name : "Student " + index, people);
            float scale = adult ? .86f : .68f;
            string[] clothes = { "ED987A", "619EBE", "DCA7CC", "E4BF66", "81B7A1", "9E9EC9" };
            Shape(p, "Body", PrimitiveType.Capsule, new Vector3(0, .42f, 0), new Vector3(.31f, .3f, .23f), director ? "347E83" : adult ? "426B86" : clothes[index % clothes.Length]);
            Shape(p, "Head", PrimitiveType.Sphere, new Vector3(0, .85f, 0), Vector3.one * .29f, index % 3 == 0 ? "B68460" : "EAC49B");
            string hair = director ? HairColor(app.State.Director.Hair) : index % 3 == 0 ? "5A443B" : index % 3 == 1 ? "C8A369" : "483D38";
            Shape(p, "Hair", PrimitiveType.Sphere, new Vector3(0, .93f, .015f), new Vector3(.31f, .17f, .3f), hair);
            if (director && app.State.Director.Gender != "Male")
                Shape(p, "Hair back", PrimitiveType.Sphere, new Vector3(0, .8f, .1f), new Vector3(.33f, app.State.Director.Gender == "Female" ? .38f : .25f, .18f), hair);
            for (int side = -1; side <= 1; side += 2)
            {
                Shape(p, "Eye", PrimitiveType.Sphere, new Vector3(side * .055f, .86f, -.133f), new Vector3(.028f, .036f, .012f), "30414A");
                Box(p, "Leg", side * .078f, .18f, 0, .105f, .3f, .11f, "3C657E");
                Box(p, "Shoe", side * .078f, .045f, -.02f, .14f, .075f, .2f, "E6E3D4");
            }
            if (!adult) Box(p, "Backpack", 0, .51f, .14f, .23f, .3f, .14f, "CB746F");
            p.localScale = Vector3.one * scale;
            p.localPosition = new Vector3(CampusGrid.Entrance.X + .5f + (index % 4) * .1f, .15f, CampusGrid.Entrance.Y + .5f);
            CampusPerson person = p.gameObject.AddComponent<CampusPerson>(); person.Initialize(app, index, adult, employee);
            agents.Add(person);
        }
        public static string HairColor(string name)
        { return name == "Black" ? "403B37" : name == "Blonde" ? "E0C279" : name == "Red" ? "B97453" : name == "Silver" ? "BFCBD0" : "796047"; }
        public void ClearGhost()
        {
            if (ghost != null) { ghost.gameObject.SetActive(false); Destroy(ghost.gameObject); ghost = null; }
            ghostCell = new Cell(-1, -1); ghostKind = null;
        }
        public void ShowGhost(Cell cell)
        {
            if (!app.BuildKind.HasValue) { ClearGhost(); return; }
            if (cell.Equals(ghostCell) && ghostKind == app.BuildKind && ghostRotated == app.Rotated) return;
            ClearGhost(); ghostCell = cell; ghostKind = app.BuildKind; ghostRotated = app.Rotated;
            var spec = Catalog.Get(app.BuildKind.Value);
            int w = app.Rotated ? spec.Height : spec.Width, h = app.Rotated ? spec.Width : spec.Height;
            bool valid = app.Simulation.CanBuild(app.BuildKind.Value, cell.X, cell.Y, app.Rotated).Success;
            ghost = Group("Build preview", transform);
            string color = valid ? "83DDAB" : "EB8E87";
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++)
                Box(ghost, "Footprint", cell.X + x + .5f, .22f, cell.Y + y + .5f, .9f, .07f, .9f, color);
            Label(ghost, app.RoomName(spec.Kind) + " · " + spec.Cost + " €", new Vector3(cell.X + w / 2f, 1.5f, cell.Y + h / 2f), .13f, valid ? "27664B" : "A94545", true);
        }
        private void OnDestroy() { foreach (Material m in materials.Values) Destroy(m); }
    }

    public sealed class CampusBillboard : MonoBehaviour
    {
        private void LateUpdate() { if (Camera.main != null) transform.rotation = Camera.main.transform.rotation; }
    }

    public sealed class CampusPerson : MonoBehaviour
    {
        private SchoolApp app;
        private int index, lastPhase = -1;
        private bool adult;
        private Employee employee;
        private List<Vector3> waypoints = new List<Vector3>();
        private int waypoint;
        private float wait, gait;
        private Cell corridor = CampusGrid.Entrance;
        private Vector3 indoorExit;
        private bool indoors;
        public void Initialize(SchoolApp owner, int number, bool isAdult, Employee person)
        { app = owner; index = number; adult = isAdult; employee = person; wait = (index % 11) * .23f; }
        private void Update()
        {
            if (app.Paused || app.UI == null || app.UI.HasModal || !app.State.HasDirector || !app.Simulation.CanOperate) return;
            float dt = Time.deltaTime * app.Speed;
            int phase = (app.State.ClockMinute - 480) / 60;
            if (waypoint < waypoints.Count)
            {
                Vector3 target = waypoints[waypoint], delta = target - transform.localPosition;
                float step = dt * (adult ? 1.2f : 1.45f);
                if (delta.magnitude < step) { transform.localPosition = target; waypoint++; }
                else
                {
                    transform.localPosition += delta.normalized * step;
                    if (delta.sqrMagnitude > .001f) transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(-delta.normalized), dt * 9);
                }
                gait += dt * 11;
                Vector3 p = transform.localPosition; p.y = .15f + Mathf.Abs(Mathf.Sin(gait)) * .035f; transform.localPosition = p;
                return;
            }
            wait -= dt;
            if (wait > 0 && phase == lastPhase) return;
            lastPhase = phase; wait = 5 + index % 9;
            RoomKind kind = phase == 4 ? RoomKind.Canteen : phase >= 7 ? RoomKind.Garden : adult && employee.Role != StaffRole.Teacher ? RoomKind.Staffroom : RoomKind.Classroom;
            var rooms = app.State.Rooms.Where(r => r.Kind == kind).ToList();
            if (rooms.Count == 0) rooms = app.State.Rooms.Where(r => r.Kind == RoomKind.Classroom).ToList();
            if (rooms.Count == 0) return;
            Room chosen = rooms[(index + phase / 3) % rooms.Count]; Cell door, inside;
            if (!CampusGrid.TryDoor(chosen, CampusGrid.Reachable(CampusGrid.Corridors(app.State)), out door, out inside)) return;
            var path = CampusGrid.Path(app.State, corridor, door);
            waypoints.Clear(); waypoint = 0;
            if (indoors) waypoints.Add(indoorExit);
            foreach (Cell c in path) waypoints.Add(new Vector3(c.X + .5f + (index % 3 - 1) * .11f, .15f, c.Y + .5f));
            indoorExit = new Vector3(inside.X + .5f, .15f, inside.Y + .5f);
            waypoints.Add(indoorExit);
            float ix = chosen.X + .55f + (index % 4) * (chosen.Width - 1.1f) / 4;
            float iz = chosen.Y + .55f + ((index / 4) % 3) * (chosen.Height - 1.1f) / 3;
            waypoints.Add(new Vector3(ix, .15f, iz));
            corridor = door; indoors = true;
        }
    }
}
