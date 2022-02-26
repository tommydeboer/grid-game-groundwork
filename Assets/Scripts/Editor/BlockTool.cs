using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GridGame.Blocks;
using GridGame.DevTools;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace GridGame.Editor
{
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
    [EditorTool(TITLE)]
    public class BlockTool : EditorTool
    {
        const string TITLE = "Rectangle Tool";
        public override GUIContent toolbarIcon => iconContent;
        GUIContent iconContent;

        State state;
        LevelEditor levelEditor;
        ModeModifiers modeModifiers;

        GridSelection selection;
        Vector3Int mousePos;

        int lastRenderedFrame;
        bool drawMeshes;

        void OnEnable()
        {
            iconContent = new GUIContent
            {
                image = EditorGUIUtility.IconContent("PreMatCube").image,
                text = TITLE,
                tooltip = TITLE
            };
            state = EditorAssets.State;

            modeModifiers = new ModeModifiers(state);
            modeModifiers.Register(EventModifiers.Shift, Mode.Pick);
            modeModifiers.Register(EventModifiers.Control, Mode.Erase);
            modeModifiers.Register((EventModifiers.Control | EventModifiers.Shift), Mode.Select);
        }

        public override void OnActivated()
        {
            Selection.activeGameObject = null;
            levelEditor = LevelEditor.ShowWindow();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView)
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
            DecideDrawMeshes(eventType);

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
            drawMeshes = false;
        }

        /// <summary>
        /// Meshes drawn with Graphics.DrawMesh are not automatically cleared outside of MonoBehaviour lifecycles.
        /// This methods clears the meshes at the right time and tells the rest of the tool to draw new meshes during
        /// the current event.
        /// </summary>
        void DecideDrawMeshes(EventType eventType)
        {
            if (eventType == EventType.Layout)
            {
                // Force the view to update.
                EditorUtility.SetDirty(this);

                // Only update if we know that the queue was cleared.
                if (lastRenderedFrame != Time.renderedFrameCount)
                {
                    drawMeshes = true;
                    lastRenderedFrame = Time.renderedFrameCount;
                }
            }
        }

        void HandleModifiers(Event e)
        {
            if (e.alt)
            {
                // Is rotating camera, stop selection
                selection = null;
            }

            modeModifiers.Evaluate(e.modifiers);
        }

        void HandleShortcuts(Event e, EventType eventType)
        {
            if (eventType == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case >= (KeyCode)49 and <= (KeyCode)57:
                        state.SelectedPrefabId = (int)e.keyCode - 49;
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
                    case KeyCode.KeypadPlus:
                        state.IncreaseRotation();
                        e.Use();
                        return;
                    case KeyCode.KeypadMinus:
                        state.DecreaseRotation();
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
                            PickPrefab(mousePos);
                            break;
                        case 0 when state.Mode == Mode.Create:
                            selection = new GridSelection(mousePos, state.SpawnHeight);
                            break;
                        case 0 when state.Mode == Mode.Select:
                            selection = new GridSelection(mousePos);
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
                    selection.EndPos = Mouse.GetPositionOnPlane(e.mousePosition, selection.Plane);
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

            switch (state.Mode)
            {
                case Mode.Create:
                {
                    ApplyCreateSelection(state.CurrentLevel);
                    break;
                }
                case Mode.Erase:
                {
                    // TODO replace with single check in a box
                    selection.ForEach(pos => state.CurrentLevel.ClearAt(pos));
                    break;
                }
                case Mode.Select:
                {
                    ApplySelectSelection();
                    break;
                }
                case Mode.Pick:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            selection = null;
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
                selection.ForEach(pos => level.CreateAt(state.SelectedPrefab, pos, state.SpawnRotation));
            }
        }

        void ApplySelectSelection()
        {
            var objects = new List<GameObject>();
            selection.ForEach(pos =>
            {
                Block blockAtPos = Utils.GetBlockAtPos(pos);
                if (blockAtPos)
                {
                    objects.Add(blockAtPos.gameObject);
                }
            });

            // ReSharper disable once CoVariantArrayConversion
            Selection.objects = objects.ToArray();
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
                case Mode.Select:
                    Draw.DrawWireBox(selection.Bounds, Color.white);
                    break;
                case Mode.Pick:
                    throw new InvalidOperationException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void DrawEraseSelection()
        {
            if (drawMeshes)
            {
                Vector3[] intersections = selection.Intersections;
                if (intersections.Length > 0)
                {
                    foreach (Vector3 pos in intersections)
                    {
                        Draw.DrawRedOverlayBox(pos, Vector3.one * 1.05f);
                    }
                }
            }

            Draw.DrawWireBox(selection.Bounds, Color.red);
        }

        void DrawCreateSelection()
        {
            if (!drawMeshes) return;

            selection.ForEach(pos => Draw.DrawPrefabPreview(pos, state.SpawnRotation, state.SelectedPrefab));

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
                    if (drawMeshes) Draw.DrawPrefabPreview(pos, state.SpawnRotation, state.SelectedPrefab);
                    break;
                case Mode.Erase:
                    Draw.DrawWireCube(pos, Color.red);
                    break;
                case Mode.Pick:
                    Draw.DrawWireCube(pos, Color.yellow);
                    break;
                case Mode.Select:
                    Draw.DrawWireCube(pos, Color.white);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void PickPrefab(Vector3Int mousePos)
        {
            var block = Utils.GetBlockAtPos(mousePos);
            if (block)
            {
                string blockName = block.transform.name;
                for (int i = 0; i < state.Prefabs.Count; i++)
                {
                    if (state.Prefabs[i].transform.name == blockName)
                    {
                        state.SelectedPrefabId = i;
                    }
                }
            }

            modeModifiers.Reset();
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
            ToolManager.SetActiveTool<BlockTool>();
            EditorAssets.State.Mode = Mode.Create;
        }
    }
}