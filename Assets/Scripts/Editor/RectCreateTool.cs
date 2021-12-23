using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Editor
{
    class GridSelection
    {
        public Vector3Int StartPos { get; }
        Vector3Int currentPos;
        Plane selectionPlane;

        public GridSelection(Vector3Int startPos)
        {
            StartPos = startPos;
        }
    }

    [EditorTool(TITLE)]
    public class RectCreateTool : EditorTool
    {
        GUIContent iconContent;

        const string TITLE = "Rectangle Tool";
        LevelEditor window;
        State state;

        public override GUIContent toolbarIcon => iconContent;

        GridSelection selection = null;

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

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView))
            {
                return;
            }

            Event e = Event.current;
            EventType eventType = GetEventType(e);
            Vector3Int pos = GetMousePosition(e.mousePosition);

            if (eventType == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    selection = new GridSelection(pos);
                }
                else if (e.button == 1 && selection != null)
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


            if (selection != null)
            {
                DrawSelection(pos);
            }
            else
            {
                DrawCursor(pos);
            }

            window.Repaint();
        }

        void DrawSelection(Vector3Int currentPos)
        {
            var from = selection.StartPos;
            var to = currentPos;
            int minX = Math.Min(from.x, to.x);
            int maxX = Math.Max(from.x, to.x);
            int minZ = Math.Min(from.z, to.z);
            int maxZ = Math.Max(from.z, to.z);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    DrawCursor(new Vector3Int(x, from.y, z));
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

        Vector3Int GetMousePosition(Vector3 mousePos)
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