using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class SceneViewInteraction
    {
        readonly LevelEditor levelEditor;
        readonly LevelEditorValues values;
        readonly LevelFactory levelFactory;

        bool isHoldingAlt;
        bool mouseButtonDown;
        Vector3 drawPos;
        Vector2 mousePosOnClick;

        public SceneViewInteraction(LevelEditor levelEditor, LevelEditorValues values)
        {
            this.levelEditor = levelEditor;
            this.values = values;
            levelFactory = new LevelFactory();
        }

        public void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            if (e.modifiers != EventModifiers.None)
            {
                isHoldingAlt = true;
                mouseButtonDown = false;
            }
            else
            {
                isHoldingAlt = false;
            }

            Vector3 currentPos = GetPosition(e.mousePosition);
            if (values.SelectedPrefabId != 1)
            {
                currentPos += (Vector3.back * values.SpawnHeight);
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
                    values.SpawnHeight += deltaY;
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
                    if (e.button == 0 && values.SelectedPrefabId != 0)
                    {
                        e.Use();
                        levelEditor.Refresh();
                        drawPos = currentPos;
                        levelFactory.GetLevel(values.CurrentLevel)
                            .CreateAt(GetSelectedPrefab(), Utils.Vec3ToInt(drawPos), values.SpawnRotation);
                        levelEditor.Refresh();
                        mouseButtonDown = true;
                        mousePosOnClick = e.mousePosition;
                    }
                    else if (e.button == 1)
                    {
                        values.SelectedPrefabId = 0;
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                        if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
                        {
                            for (int i = 0; i < values.Prefabs.Count; i++)
                            {
                                if (values.Prefabs[i].transform.name == hit.transform.parent.name)
                                {
                                    values.SelectedPrefabId = i + 2;
                                }
                            }
                        }
                    }
                }
                else if (mouseButtonDown)
                {
                    if (Vector2.Distance(mousePosOnClick, e.mousePosition) > 10f)
                    {
                        if (!Utils.VectorRoughly2D(drawPos, currentPos, 0.75f))
                        {
                            drawPos = Utils.Vec3ToInt(currentPos);
                            levelFactory.GetLevel(values.CurrentLevel)
                                .CreateAt(GetSelectedPrefab(), drawPos, values.SpawnRotation);
                            levelEditor.Refresh();
                            mousePosOnClick = e.mousePosition;
                        }
                    }
                }
            }

            LevelGizmo.UpdateGizmo(currentPos, values.GizmoColor);
            LevelGizmo.Enable(values.SelectedPrefabId != 0);
            view.Repaint();
            levelEditor.Repaint();
        }

        Vector3 GetPosition(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 pos = hit.point + (hit.normal * 0.5f);
                if (values.SelectedPrefabId == 1)
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

        GameObject GetSelectedPrefab()
        {
            if (values.SelectedPrefabId == 1)
            {
                return null;
            }

            return values.Prefabs[values.SelectedPrefabId - 2];
        }
    }
}