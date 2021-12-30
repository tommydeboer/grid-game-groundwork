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
        LevelEditor levelEditor;
        ModeModifier pickModifier;
        ModeModifier eraseModifier;

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
            state = EditorAssets.State;
            levelFactory = new LevelFactory();
            pickModifier = new ModeModifier(state, EventModifiers.Shift, Mode.Pick);
            eraseModifier = new ModeModifier(state, EventModifiers.Control, Mode.Erase);
        }

        public override void OnActivated()
        {
            Selection.activeGameObject = null;
            levelEditor = LevelEditor.ShowWindow();
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
            HandleModifiers(e);

            HandleShortcuts(e, eventType);

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
                levelEditor.Repaint();
            }
        }

        void HandleModifiers(Event e)
        {
            if (e.alt)
            {
                // Is rotating camera, stop selection
                selection = null;
            }

            pickModifier.Evaluate(e.modifiers);
            eraseModifier.Evaluate(e.modifiers);
        }

        void HandleShortcuts(Event e, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case >= (KeyCode) 49 and <= (KeyCode) 57:
                        state.SelectedPrefabId = (int) e.keyCode - 49;
                        state.Mode = Mode.Create;
                        e.Use();
                        return;
                    case KeyCode.PageUp:
                        state.SpawnHeight++;
                        e.Use();
                        return;
                    case KeyCode.PageDown:
                        state.SpawnHeight--;
                        e.Use();
                        return;
                    case KeyCode.Tab:
                        state.SetNextMode();
                        e.Use();
                        return;
                }
            }
        }

        void HandleCursorInput(Event e, EventType eventType)
        {
            mousePos = Mouse.GetPosition(e.mousePosition, state.Mode == Mode.Create);
            ApplyMouseVerticalOffset();

            switch (eventType)
            {
                case EventType.MouseDown:
                {
                    switch (e.button)
                    {
                        case 0 when state.Mode == Mode.Pick:
                            PickPrefab(e.mousePosition);
                            break;
                        case 0 when state.Mode == Mode.Create:
                            selection = new GridSelection(mousePos, state.SpawnHeight);
                            break;
                        case 0:
                            selection = new GridSelection(mousePos);
                            break;
                    }

                    break;
                }
            }
        }

        void ApplyMouseVerticalOffset()
        {
            if (state.Mode == Mode.Create && state.SpawnHeight > 0)
            {
                mousePos += (Vector3Int.up * state.SpawnHeight);
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
            if (state.Mode == Mode.Create && selection.Intersections.Length > 0)
            {
                selection = null;
                return;
            }

            levelEditor.Refresh();
            Level level = levelFactory.GetLevel(state.CurrentLevel);

            switch (state.Mode)
            {
                case Mode.Create:
                {
                    ApplyCreateSelection(level);
                    break;
                }
                case Mode.Erase:
                {
                    // TODO replace with single check in a box
                    selection.ForEach(pos => level.ClearAt(pos));
                    break;
                }
                case Mode.Pick:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            selection = null;
            levelEditor.Refresh();
        }

        void ApplyCreateSelection(Level level)
        {
            bool proceed = true;
            if (selection.Count > 1000)
            {
                proceed = EditorUtility.DisplayDialog(
                    "Attention!",
                    $"About to create {selection.Count} objects. Are you sure?",
                    "Yes", "Cancel");
            }

            if (proceed)
            {
                selection.ForEach(pos => level.CreateAt(state.SelectedPrefab, pos, Vector3.zero));
            }
        }


        void DrawSelection()
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    DrawCreateSelection();
                    break;
                case Mode.Erase:
                    DrawEraseSelection();

                    break;
                case Mode.Pick:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void DrawEraseSelection()
        {
            Vector3[] intersections = selection.Intersections;
            if (intersections.Length > 0)
            {
                foreach (Vector3 pos in intersections)
                {
                    Draw.DrawRedOverlayBox(pos, Vector3.one);
                }
            }

            Draw.DrawWireBox(selection.Bounds, Color.red);
        }

        void DrawCreateSelection()
        {
            selection.ForEach(pos => Draw.DrawPrefabPreview(pos, state.SelectedPrefab));

            if (selection.Intersections.Length > 0)
            {
                var bounds = selection.Bounds;
                bounds.Expand(Vector3.one * 0.05f);
                Draw.DrawRedOverlayBox(bounds);
            }
        }

        void DrawCursor(Vector3Int pos)
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    Draw.DrawHeightIndicator(pos, state.SpawnHeight);
                    Draw.DrawPrefabPreview(pos, state.SelectedPrefab);
                    break;
                case Mode.Erase:
                    Draw.DrawWireCube(pos, Color.red);
                    break;
                case Mode.Pick:
                    Draw.DrawWireCube(pos, Color.yellow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void PickPrefab(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                for (int i = 0; i < state.Prefabs.Count; i++)
                {
                    if (state.Prefabs[i].transform.name == hit.transform.parent.name)
                    {
                        state.SelectedPrefabId = i;
                    }
                }
            }

            pickModifier.Reset();
            state.Mode = Mode.Create;
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
            EditorAssets.State.Mode = Mode.Create;
        }
    }
}