using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Editor
{
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
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
                    DrawSelection();
                }
                else
                {
                    DrawCursor(mousePos);
                }

                window.Repaint();
            }
        }

        void HandleCursorInput(Event e, EventType eventType)
        {
            mousePos = GetMouseCursorPosition(e.mousePosition);
            switch (eventType)
            {
                case EventType.KeyDown:
                {
                    if (e.keyCode == KeyCode.Tab)
                    {
                        state.SetNextMode();
                        e.Use();
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    if (e.button == 0)
                    {
                        selection = new GridSelection(mousePos);
                    }

                    break;
                }
            }
        }

        void HandleSelectionInput(Event e, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.KeyDown:
                {
                    if (e.keyCode == KeyCode.Escape)
                    {
                        selection = null;
                    }

                    e.Use();
                    break;
                }
                case EventType.MouseDown:
                {
                    if (e.button == 1)
                    {
                        selection = null;
                    }

                    e.Use();
                    break;
                }
                case EventType.MouseUp:
                {
                    if (e.button == 0)
                    {
                        selection = null;
                    }

                    e.Use();
                    break;
                }
                case EventType.MouseDrag:
                    mousePos = GetMouseSelectionPosition(e.mousePosition);
                    selection.CurrentPos = mousePos;
                    break;
                case EventType.ScrollWheel:
                    selection.Height += (e.delta.y < 0) ? 1 : -1;
                    e.Use();
                    break;
            }
        }

        void DrawSelection()
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    selection.ForEach(DrawCursor);
                    break;
                case Mode.Erase:
                    Draw.DrawWireBox(selection.MinCorner, selection.MaxCorner, Color.red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void DrawCursor(Vector3Int pos)
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    Draw.DrawPrefabPreview(pos, state.SelectedPrefab);
                    break;
                case Mode.Erase:
                    Draw.DrawWireCube(pos, Color.red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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