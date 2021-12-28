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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



    }
}