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

        bool isHoldingAlt;
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

            if (e.modifiers == EventModifiers.Alt)
            {
                isHoldingAlt = true;
                mouseButtonDown = false;
            }
            else
            {
                isHoldingAlt = false;
            }

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

            if (e.isKey && e.keyCode == KeyCode.P)
            {
                EditorApplication.ExecuteMenuItem("Edit/Play");
            }

            if (isHoldingAlt)
            {
                if (eventType == EventType.ScrollWheel)
                {
                    int deltaY = (e.delta.y < 0) ? -1 : 1;
                    state.SpawnHeight += deltaY;
                    currentPos += (Vector3.back * deltaY);
                    e.Use();
                }
            }
            else
            {
                if (eventType == EventType.MouseUp)
                {
                    mouseButtonDown = false;
                }

                if (eventType == EventType.MouseDown)
                {
                    if (e.button == 0 && state.PlacementMode != PlacementMode.None)
                    {
                        // placing and erasing
                        e.Use();
                        levelEditor.Refresh();
                        drawPos = currentPos;

                        Level level = levelFactory.GetLevel(state.CurrentLevel);
                        switch (state.PlacementMode)
                        {
                            case PlacementMode.Create:
                                level
                                    .CreateAt(state.SelectedPrefab, Utils.Vec3ToInt(drawPos), state.SpawnRotation);
                                break;
                            case PlacementMode.Erase:
                                level.ClearAt(Utils.Vec3ToInt(drawPos));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        levelEditor.Refresh();
                        mouseButtonDown = true;
                        mousePosOnClick = e.mousePosition;
                    }
                    else if (e.button == 1)
                    {
                        // copy prefab type?
                        // values.SelectedPrefabId = 0;
                        // Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                        //
                        // if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
                        // {
                        //     for (int i = 0; i < values.Prefabs.Count; i++)
                        //     {
                        //         if (values.Prefabs[i].transform.name == hit.transform.parent.name)
                        //         {
                        //             values.SelectedPrefabId = i + 2;
                        //         }
                        //     }
                        // }
                    }
                }
                else if (mouseButtonDown)
                {
                    // clicking and dragging?
                    if (Vector2.Distance(mousePosOnClick, e.mousePosition) > 10f)
                    {
                        if (!Utils.VectorRoughly2D(drawPos, currentPos, 0.75f))
                        {
                            drawPos = Utils.Vec3ToInt(currentPos);
                            Level level = levelFactory.GetLevel(state.CurrentLevel);
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
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            levelEditor.Refresh();
                            mousePosOnClick = e.mousePosition;
                        }
                    }
                }
            }

            LevelGizmo.UpdateGizmo(currentPos, state.GizmoColor);
            LevelGizmo.Enable(state.PlacementMode != PlacementMode.None);
            view.Repaint();
            levelEditor.Repaint();
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