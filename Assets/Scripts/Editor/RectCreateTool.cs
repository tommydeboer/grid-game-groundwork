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
        LevelFactory levelFactory;

        GridSelection selection;
        Vector3Int mousePos;
        LevelEditor levelEditor;


        void OnEnable()
        {
            iconContent = new GUIContent
            {
                image = EditorGUIUtility.IconContent("PreMatCube").image,
                text = TITLE,
                tooltip = TITLE
            };
            state = LevelEditor.state;
            levelFactory = new LevelFactory();
        }

        public override void OnActivated()
        {
            Selection.activeGameObject = null;
            levelEditor = EditorWindow.GetWindow<LevelEditor>();
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
            if (e.alt)
            {
                // Is rotating camera
                selection = null;
                return;
            }

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
            mousePos = Mouse.GetPosition(e.mousePosition, state.Mode == Mode.Create);
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
                        ApplySelection();
                    }

                    e.Use();
                    break;
                }
                case EventType.MouseDrag:
                    selection.CurrentPos = Mouse.GetPositionOnPlane(e.mousePosition, selection.Plane);
                    break;
                case EventType.ScrollWheel:
                    selection.Height += (e.delta.y < 0) ? 1 : -1;
                    e.Use();
                    break;
            }
        }

        void ApplySelection()
        {
            levelEditor.Refresh();
            Level level = levelFactory.GetLevel(state.CurrentLevel);

            switch (state.Mode)
            {
                case Mode.Create:
                {
                    selection.ForEach(pos => level.CreateAt(state.SelectedPrefab, pos, Vector3.zero));
                    break;
                }
                case Mode.Erase:
                {
                    selection.ForEach(pos => level.ClearAt(pos));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            selection = null;
            levelEditor.Refresh();
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