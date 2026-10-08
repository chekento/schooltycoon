using UnityEngine;
using UnityEngine.EventSystems;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon
{
    public sealed class CampusCamera : MonoBehaviour
    {
        public Camera View { get; private set; }
        private SchoolApp app;
        private Vector3 focus = new Vector3(15, 0, 11);
        private float yaw = -36, zoom = 16;
        private Vector2 press, previous;
        private bool dragging, startedOnUI;
        public void Initialize(SchoolApp owner)
        {
            app = owner;
            View = gameObject.AddComponent<Camera>(); View.tag = "MainCamera";
            gameObject.AddComponent<AudioListener>();
            View.orthographic = true; View.nearClipPlane = .1f; View.farClipPlane = 200;
            View.backgroundColor = new Color(.78f, .9f, .95f); View.clearFlags = CameraClearFlags.SolidColor;
            Snap();
        }
        public void Home() { focus = new Vector3(15, 0, 11); zoom = 16; yaw = -36; Snap(); }
        public void Zoom(float amount) { zoom = Mathf.Clamp(zoom + amount, 5.5f, 25); Snap(); }
        public void Orbit(float direction) { yaw += direction * 45; Snap(); }
        private bool OverUI(int id = -1)
        { return EventSystem.current != null && (id < 0 ? EventSystem.current.IsPointerOverGameObject() : EventSystem.current.IsPointerOverGameObject(id)); }
        private void Update()
        {
            if (app.UI == null || app.UI.HasModal) return;
            if (Input.touchCount > 0) { TouchInput(); return; }
            Vector2 p = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            { press = previous = p; dragging = false; startedOnUI = OverUI(); }
            if (!startedOnUI && Input.GetMouseButton(0))
            {
                if ((p - press).magnitude > 12) dragging = true;
                if (dragging) Pan(p - previous);
                previous = p;
            }
            if (!startedOnUI && Input.GetMouseButtonUp(0) && !dragging) Click(p);
            if (!OverUI())
            {
                if (Input.GetMouseButton(2) || Input.GetMouseButton(1)) Pan(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 12);
                Zoom(-Input.mouseScrollDelta.y * .8f);
                Vector3 right = transform.right; right.y = 0; right.Normalize();
                Vector3 forward = Vector3.Cross(right, Vector3.up);
                focus += (right * Input.GetAxisRaw("Horizontal") + forward * Input.GetAxisRaw("Vertical")) * Time.unscaledDeltaTime * 10;
                if (Input.GetKeyDown(KeyCode.Q)) Orbit(-1);
                if (Input.GetKeyDown(KeyCode.E)) Orbit(1);
                if (Input.GetKeyDown(KeyCode.R)) app.Rotate();
                if (Input.GetKeyDown(KeyCode.Escape)) app.ChooseBuild(null);
                if (Input.GetKeyDown(KeyCode.Space)) app.TogglePause();
                Cell c; if (Ground(p, out c)) app.Campus.ShowGhost(c); else app.Campus.ClearGhost();
            }
            else app.Campus.ClearGhost();
            Snap();
        }
        private void TouchInput()
        {
            if (Input.touchCount == 2)
            {
                Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
                if (OverUI(a.fingerId) || OverUI(b.fingerId)) return;
                Vector2 pa = a.position - a.deltaPosition, pb = b.position - b.deltaPosition;
                Zoom(((pa - pb).magnitude - (a.position - b.position).magnitude) * .025f);
                dragging = true; return;
            }
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { press = previous = t.position; dragging = false; startedOnUI = OverUI(t.fingerId); }
            if (startedOnUI) return;
            if (t.phase == TouchPhase.Moved)
            {
                if ((t.position - press).magnitude > 12) dragging = true;
                if (dragging) Pan(t.position - previous);
                previous = t.position;
            }
            if (t.phase == TouchPhase.Ended && !dragging) Click(t.position);
            Cell c; if (Ground(t.position, out c)) app.Campus.ShowGhost(c);
        }
        private void Pan(Vector2 delta)
        {
            Vector3 right = transform.right; right.y = 0; right.Normalize();
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            focus -= (right * delta.x + forward * delta.y * 1.45f) * (zoom * 2 / Screen.height);
            Snap();
        }
        private bool Ground(Vector2 screen, out Cell c)
        {
            Ray ray = View.ScreenPointToRay(screen); float distance;
            if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out distance))
            { Vector3 p = ray.GetPoint(distance); c = new Cell(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z)); return c.X >= 0 && c.X < CampusGrid.Width && c.Y >= 0 && c.Y < CampusGrid.Height; }
            c = default(Cell); return false;
        }
        private void Click(Vector2 p) { Cell cell; if (Ground(p, out cell)) app.BuildAt(cell); }
        private void Snap()
        {
            focus.x = Mathf.Clamp(focus.x, 1, CampusGrid.Width - 1); focus.z = Mathf.Clamp(focus.z, 1, CampusGrid.Height - 1);
            transform.rotation = Quaternion.Euler(48, yaw, 0);
            transform.position = focus - transform.forward * 55;
            View.orthographicSize = zoom;
        }
    }
}
