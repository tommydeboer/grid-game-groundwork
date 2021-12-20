using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public class SceneViewInteraction
    {
        readonly LevelEditor levelEditor;
        readonly State state;
        readonly LevelFactory levelFactory;

        bool mouseButtonDown;
        Vector3 drawPos;
        Vector2 mousePosOnClick;

        public SceneViewInteraction(LevelEditor levelEditor, State state)
        {
            this.levelEditor = levelEditor;
            this.state = state;
            levelFactory = new LevelFactory();
        }

        public void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            if (e.isKey)
            {
                HandleShortcuts(e);
            }
            else
            {
                HandleMouse(view, e);
            }

            levelEditor.Repaint();
        }

        void HandleMouse(EditorWindow view, Event e)
        {
            Vector3 pos = GetGizmoPosition(e.mousePosition);
            EventType eventType = GetEventType(e);
            SetLeftMouseButtonState(view, e, eventType);

            if (e.modifiers == EventModifiers.Alt && eventType == EventType.ScrollWheel)
            {
                pos = SetSpawnHeight(e, pos);
                e.Use();
            }

            if (state.Mode != Mode.None)
            {
                if (eventType == EventType.MouseDown)
                {
                    switch (e.button)
                    {
                        case 0:
                            HandleLeftClick(e, pos);
                            break;
                        case 2:
                            HandleMiddleClick(e);
                            break;
                    }
                }
                else if (mouseButtonDown && eventType == EventType.MouseDrag)
                {
                    HandleDrag(e, pos);
                }

                e.Use();
                DrawGizmo(pos);
                view.Repaint();
            }
        }

        void SetLeftMouseButtonState(Object view, Event e, EventType eventType)
        {
            if (EditorWindow.mouseOverWindow != view)
            {
                mouseButtonDown = false;
            }
            else if (eventType == EventType.MouseUp && e.button == 0)
            {
                mouseButtonDown = false;
            }
        }

        void HandleShortcuts(Event e)
        {
            switch (e.keyCode)
            {
                case >= (KeyCode) 49 and <= (KeyCode) 57:
                    state.SelectedPrefabId = (int) e.keyCode - 49;
                    state.Mode = Mode.Create;
                    e.Use();
                    return;
                case KeyCode.Minus:
                    state.Mode = Mode.Erase;
                    e.Use();
                    return;
                case KeyCode.Escape:
                    state.Mode = Mode.None;
                    e.Use();
                    return;
                case KeyCode.P:
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                    e.Use();
                    return;
            }
        }

        void DrawGizmo(Vector3 currentPos)
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

        void HandleMiddleClick(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

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
        }

        void HandleLeftClick(Event e, Vector3 currentPos)
        {
            // placing and erasing
            e.Use();
            levelEditor.Refresh();
            drawPos = currentPos;

            Level level = levelFactory.GetLevel(state.CurrentLevel);
            CreateOrErase(level);

            levelEditor.Refresh();
            mouseButtonDown = true;
            mousePosOnClick = e.mousePosition;
        }

        void HandleDrag(Event e, Vector3 currentPos)
        {
            if (Vector2.Distance(mousePosOnClick, e.mousePosition) > 10f)
            {
                if (!Utils.VectorRoughly2D(drawPos, currentPos, 0.75f))
                {
                    drawPos = Utils.Vec3ToInt(currentPos);
                    Level level = levelFactory.GetLevel(state.CurrentLevel);
                    CreateOrErase(level);

                    levelEditor.Refresh();
                    mousePosOnClick = e.mousePosition;
                }
            }
        }

        void CreateOrErase(Level level)
        {
            switch (state.Mode)
            {
                case Mode.Create:
                    level
                        .CreateAt(state.SelectedPrefab, Utils.Vec3ToInt(drawPos),
                            state.SpawnRotation);
                    break;
                case Mode.Erase:
                    level.ClearAt(Utils.Vec3ToInt(drawPos));
                    break;
                case Mode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Vector3 SetSpawnHeight(Event e, Vector3 currentPos)
        {
            int deltaY = (e.delta.y < 0) ? -1 : 1;
            state.SpawnHeight += deltaY;
            currentPos += (Vector3.back * deltaY);
            return currentPos;
        }

        Vector3 GetGizmoPosition(Vector3 mousePos)
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

                return Utils.Vec3ToInt(pos);
            }

            return Vector3.zero;
        }

        static EventType GetEventType(Event e)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);
            return eventType;
        }
    }
}