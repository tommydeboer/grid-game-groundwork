using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                return;
            }

            if (view)
            {
                HandleMouse(view, e);
            }

            levelEditor.Repaint();
        }

        void HandleMouse(EditorWindow view, Event e)
        {
            Vector3 currentPos = GetPosition(e.mousePosition);
            if (state.PlacementMode != PlacementMode.Erase)
            {
                currentPos += (Vector3.back * state.SpawnHeight);
                currentPos = Utils.AvoidIntersect(currentPos);
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            if (EditorWindow.mouseOverWindow != view)
            {
                mouseButtonDown = false;
            }

            if (e.modifiers == EventModifiers.Alt && eventType == EventType.ScrollWheel)
            {
                currentPos = SetSpawnHeight(e, currentPos);
                e.Use();
            }

            if (eventType == EventType.MouseUp)
            {
                mouseButtonDown = false;
            }

            if (eventType == EventType.MouseDown)
            {
                if (e.button == 0 && state.PlacementMode != PlacementMode.None)
                {
                    HandleLeftClick(e, currentPos);
                }
                else if (e.button == 1)
                {
                    HandleRightClick(e);

                    e.Use();
                }
            }
            else if (mouseButtonDown)
            {
                HandleDrag(e, currentPos);
            }

            if (state.PlacementMode != PlacementMode.None)
            {
                DrawGizmo(currentPos);
                view.Repaint();
            }
        }

        void HandleShortcuts(Event e)
        {
            switch (e.keyCode)
            {
                case >= (KeyCode) 49 and <= (KeyCode) 57:
                    state.SelectedPrefabId = (int) e.keyCode - 49;
                    state.PlacementMode = PlacementMode.Create;
                    e.Use();
                    break;
                case KeyCode.Minus:
                    state.PlacementMode = PlacementMode.Erase;
                    e.Use();
                    break;
                case KeyCode.P:
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                    e.Use();
                    break;
            }
        }

        void DrawGizmo(Vector3 currentPos)
        {
            if (state.PlacementMode == PlacementMode.Erase)
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

        void HandleRightClick(Event e)
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
            switch (state.PlacementMode)
            {
                case PlacementMode.Create:
                    level
                        .CreateAt(state.SelectedPrefab, Utils.Vec3ToInt(drawPos),
                            state.SpawnRotation);
                    break;
                case PlacementMode.Erase:
                    level.ClearAt(Utils.Vec3ToInt(drawPos));
                    break;
                case PlacementMode.None:
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

        Vector3 GetPosition(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 pos = hit.point + (hit.normal * 0.5f);
                if (state.PlacementMode == PlacementMode.Erase)
                {
                    pos = hit.transform.position;
                }

                return Utils.Vec3ToInt(pos);
            }

            Plane hPlane = new Plane(Vector3.forward, Vector3.zero);
            if (hPlane.Raycast(ray, out float distance))
            {
                return Utils.Vec3ToInt(ray.GetPoint(distance));
            }

            return Vector3.zero;
        }
    }
}