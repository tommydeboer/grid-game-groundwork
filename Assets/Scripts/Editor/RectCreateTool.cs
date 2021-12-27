using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Editor
{
    internal class GridSelection
    {
        Vector3Int StartPos { get; }
        public Vector3Int CurrentPos { get; set; }
        public Plane Plane { get; }
        public int Height { get; set; }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            CurrentPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
        }

        public void ForEach(Action<Vector3Int> fun)
        {
            int minX = Math.Min(StartPos.x, CurrentPos.x);
            int maxX = Math.Max(StartPos.x, CurrentPos.x);
            int minZ = Math.Min(StartPos.z, CurrentPos.z);
            int maxZ = Math.Max(StartPos.z, CurrentPos.z);
            int minY = Math.Min(StartPos.y, StartPos.y + Height);
            int maxY = Math.Max(StartPos.y, StartPos.y + Height);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        fun(pos);
                    }
                }
            }
        }
    }

    [EditorTool(TITLE)]
    public class RectCreateTool : EditorTool
    {
        const string TITLE = "Rectangle Tool";
        public override GUIContent toolbarIcon => iconContent;
        GUIContent iconContent;

        State state;

        GridSelection selection;
        Vector3Int mousePos;

        void OnEnable()
        {
            iconContent = new GUIContent
            {
                image = EditorGUIUtility.IconContent("PreMatCube").image,
                text = TITLE,
                tooltip = TITLE
            };
            state = LevelEditor.state;
        }

        public override void OnActivated()
        {
            Selection.activeGameObject = null;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView))
            {
                return;
            }

            Event e = Event.current;
            EventType eventType = GetEventType(e);

            Input(e, eventType);
            Paint(window, eventType);
        }

        void Input(Event e, EventType eventType)
        {
            if (selection != null)
            {
                HandleSelectionInput(e, eventType);
            }
            else
            {
                HandleCursorInput(e, eventType);
            }
        }

        void Paint(EditorWindow window, EventType eventType)
        {
            if (eventType == EventType.Repaint)
            {
                if (selection != null)
                {
                    selection.ForEach(DrawCursorElement);
                }
                else
                {
                    DrawCursorElement(mousePos);
                }

                window.Repaint();
            }
        }

        void HandleCursorInput(Event e, EventType eventType)
        {
            mousePos = GetMouseCursorPosition(e.mousePosition);
            if (eventType == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Tab)
                {
                    state.SetNextMode();
                    e.Use();
                }
            }
            else if (eventType == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    selection = new GridSelection(mousePos);
                }
            }
        }

        void HandleSelectionInput(Event e, EventType eventType)
        {
            mousePos = GetMouseSelectionPosition(e.mousePosition);
            selection.CurrentPos = mousePos;

            if (eventType == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    selection = null;
                }

                e.Use();
            }
            else if (eventType == EventType.MouseDown)
            {
                if (e.button == 1)
                {
                    selection = null;
                }

                e.Use();
            }
            else if (eventType == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    selection = null;
                }

                e.Use();
            }
            else if (e.isScrollWheel)
            {
                selection.Height += (e.delta.y < 0) ? 1 : -1;
                e.Use();
            }
        }

        void DrawCursorElement(Vector3Int pos)
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    DrawPrefabPreview(pos, state.SelectedPrefab);
                    break;
                case Mode.Erase:
                    DrawWireCube(pos, Color.red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void DrawPrefabPreview(Vector3Int pos, GameObject prefab)
        {
            Matrix4x4 poseToWorld = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter filter in filters)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                if (mat != null)
                {
                    Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
                    Matrix4x4 childToWorld = poseToWorld * childToPose;
                    Mesh mesh = filter.sharedMesh;
                    mat.SetPass(0);
                    Graphics.DrawMeshNow(mesh, childToWorld, 0);
                }
            }
        }

        static void DrawWireCube(Vector3Int pos, Color color)
        {
            Handles.color = color;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawWireCube(pos, Vector3.one);
            Handles.DrawWireCube(pos, Vector3.one * 1.01f);
            Handles.DrawWireCube(pos, Vector3.one * 0.99f);
        }

        Vector3Int GetMouseCursorPosition(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 pos = hit.point + (hit.normal * 0.5f);

                if (state.Mode == Mode.Erase)
                {
                    pos = hit.transform.position;
                }
                else
                {
                    pos += (Vector3.back * state.SpawnHeight);
                    pos = Utils.AvoidIntersect(pos);
                }

                return Vector3Int.RoundToInt(pos);
            }

            return Vector3Int.zero;
        }

        Vector3Int GetMouseSelectionPosition(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (selection != null)
            {
                if (selection.Plane.Raycast(ray, out float enter))
                {
                    return Vector3Int.RoundToInt(ray.GetPoint(enter));
                }
            }

            return Vector3Int.zero;
        }

        static EventType GetEventType(Event e)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);
            return eventType;
        }

        [Shortcut(TITLE, null, KeyCode.C)]
        static void CreateShortcut()
        {
            ToolManager.SetActiveTool<RectCreateTool>();
            LevelEditor.state.Mode = Mode.Create;
        }
    }
}