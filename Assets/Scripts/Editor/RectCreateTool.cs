using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Editor
{
    internal class GridSelection
    {
        public Vector3Int StartPos { get; }
        public Plane Plane { get; }
        public int Height { get; set; }

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
            Plane = new Plane(Vector3.up, startPos);
            Height = 0;
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

            if (selection != null)
            {
                HandleSelectionInput(e, eventType);
            }
            else
            {
                HandleCursorInput(e, eventType);
            }

            if (selection != null)
            {
                DrawSelection(mousePos);
            }
            else
            {
                DrawCursor(mousePos);
            }

            window.Repaint();
        }

        void HandleCursorInput(Event e, EventType eventType)
        {
            mousePos = GetMouseCursorPosition(e.mousePosition);

            if (eventType == EventType.MouseDown)
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
            if (e.isKey)
            {
                if (eventType == EventType.KeyDown)
                {
                    if (e.keyCode == KeyCode.Escape)
                    {
                        selection = null;
                    }
                }

                e.Use();
            }
            else if (e.isMouse)
            {
                if (eventType == EventType.MouseDown)
                {
                    if (e.button == 1)
                    {
                        selection = null;
                    }
                }
                else if (eventType == EventType.MouseUp)
                {
                    if (e.button == 0)
                    {
                        selection = null;
                    }
                }

                e.Use();
            }
            else if (e.isScrollWheel)
            {
                selection.Height += (e.delta.y < 0) ? 1 : -1;
                e.Use();
            }
        }

        void DrawSelection(Vector3Int currentPos)
        {
            var from = selection.StartPos;
            var to = currentPos;
            int minX = Math.Min(from.x, to.x);
            int maxX = Math.Max(from.x, to.x);
            int minZ = Math.Min(from.z, to.z);
            int maxZ = Math.Max(from.z, to.z);
            int minY = Math.Min(from.y, selection.Height);
            int maxY = Math.Max(from.y, selection.Height);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        DrawCursor(new Vector3Int(x, y, z));
                    }
                }
            }
        }


        void DrawCursor(Vector3Int currentPos)
        {
            if (state.Mode == Mode.Erase)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = state.GizmoColor;
            }

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawWireCube(currentPos, Vector3.one);
            Handles.DrawWireCube(currentPos, Vector3.one * 1.01f);
            Handles.DrawWireCube(currentPos, Vector3.one * 0.99f);
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

        [Shortcut(TITLE, null, KeyCode.N)]
        static void ToolShortcut()
        {
            ToolManager.SetActiveTool<RectCreateTool>();
        }
    }
}