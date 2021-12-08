#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    //TODO add play mode persistence: https://stackoverflow.com/questions/56594340/store-editor-values-between-editor-sessions
    //TODO configure prefabs via ScriptableObject instead of a text file
    public class LevelEditor : EditorWindow
    {
        int selectedPrefabId;

        int rotateInt;

        readonly string[] rotateStrings =
        {
            "0", "90", "180", "270"
        };

        int spawnHeight;
        string currentLevel;
        bool overwriteLevel;

        bool isHoldingAlt;
        bool mouseButtonDown;
        Vector3 drawPos;
        static bool playModeActive;
        Event e;

        int sceneLevelIndex;
        bool snapToGrid = true;
        bool isLoading;
        Vector3 prevPosition;
        Vector2 scrollPos;
        Color gizmoColor = Color.white;
        Vector2 mousePosOnClick;

        static EditorPrefabs editorPrefabs;
        static List<GameObject> Prefabs => editorPrefabs.Prefabs;

        Level Level
        {
            get
            {
                GameObject l = FindOrCreate(currentLevel, FindOrCreate("Levels").transform);
                l.tag = "Level";
                return new Level(l.transform);
            }
        }

        static GameObject FindOrCreate(string s, Transform parentObj = null)
        {
            GameObject go = GameObject.Find(s);
            if (go == null)
            {
                go = new GameObject();
                go.transform.name = s;
                if (parentObj != null)
                {
                    go.transform.SetParent(parentObj);
                }

                Undo.RegisterCreatedObjectUndo(go, "Create object");
            }

            return go;
        }

        readonly List<string> sceneLevels = new();

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            if (!editorPrefabs)
            {
                editorPrefabs = AssetDatabase.LoadAssetAtPath<EditorPrefabs>("Assets/Resources/EditorPrefabs.asset");
                if (editorPrefabs) return;
                editorPrefabs = CreateInstance<EditorPrefabs>();
                AssetDatabase.CreateAsset(editorPrefabs, "Assets/CustomMenuData.asset");
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(LevelEditor));
            var texture = Resources.Load<Texture2D>("ggg");
            editorWindow.titleContent = new GUIContent("Level Editor", texture);
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += SceneGUI;

            if (string.IsNullOrEmpty(currentLevel))
            {
                GameObject level = GameObject.FindGameObjectWithTag("Level");
                if (level != null)
                {
                    currentLevel = level.name;
                }
            }
        }

        void OnValidate()
        {
            EnsureTagsExist();
            Reset();
            Refresh();
        }

        void Reset()
        {
            mouseButtonDown = false;
            CreateGizmoObject();
        }

        void Refresh()
        {
            Game game = FindObjectOfType<Game>();
            if (game != null)
            {
                game.EditorRefresh();
            }

            RefreshSceneLevels();
        }

        static void CreateGizmoObject()
        {
            LevelGizmo levelGizmo = FindObjectOfType<LevelGizmo>();
            if (levelGizmo == null)
            {
                new GameObject("LevelGizmo").AddComponent<LevelGizmo>();
            }
        }

        void RefreshSceneLevels()
        {
            sceneLevels.Clear();
            GameObject[] levels = GameObject.FindGameObjectsWithTag("Level");
            foreach (GameObject l in levels)
            {
                sceneLevels.Add(l.name);
            }
        }

        static void EnsureTagsExist()
        {
            TagHelper.AddTag("Level");
            TagHelper.AddTag("Tile");
        }

        void OnGUI()
        {
            GUILayout.Label("DRAWING", EditorStyles.centeredGreyMiniLabel);

            BigSpace();

            if (!DrawLevelSelection()) return;

            BigSpace();

            DrawPlacementUI();

            BigSpace();

            gizmoColor = EditorGUILayout.ColorField("Gizmo Color:", gizmoColor);

            ///////////////// SPAWN //////////////////

            spawnHeight = EditorGUILayout.IntSlider("Spawn at height:", spawnHeight, 0, 20);

            snapToGrid = EditorGUILayout.Toggle("Snap to grid:", snapToGrid);

            BigSpace();

            SerializedObject serialObj = new SerializedObject(editorPrefabs);
            SerializedProperty serialProp = serialObj.FindProperty("prefabs");
            EditorGUILayout.PropertyField(serialProp, true);
            serialObj.ApplyModifiedProperties();
        }

        void DrawPlacementUI()
        {
            var labels = new List<string> {"None", "Erase"};
            labels.AddRange(from prefab in Prefabs select prefab.transform.name);

            GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
            selectedPrefabId = GUILayout.SelectionGrid(selectedPrefabId, labels.ToArray(), 1);

            BigSpace();

            GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);
            rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4);
        }

        bool DrawLevelSelection()
        {
            GUILayout.Label("Currently Editing: ", EditorStyles.boldLabel);

            sceneLevelIndex = 0;
            for (int i = 0; i < sceneLevels.Count; i++)
            {
                if (sceneLevels[i] == currentLevel)
                {
                    sceneLevelIndex = i;
                }
            }

            sceneLevelIndex = EditorGUILayout.Popup(sceneLevelIndex, sceneLevels.ToArray());
            currentLevel = sceneLevels[sceneLevelIndex];

            if (currentLevel == null)
            {
                return false;
            }

            return true;
        }


        static void BigSpace()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        void Update()
        {
            if (!EditorApplication.isPlaying && Selection.transforms.Length > 0 &&
                Selection.transforms[0].position != prevPosition)
            {
                foreach (Transform t in Selection.transforms)
                {
                    if (t.CompareTag("Level"))
                    {
                        currentLevel = t.name;
                    }

                    if (snapToGrid)
                    {
                        if (t.CompareTag("Level") || (t.parent != null && t.parent.CompareTag("Level")))
                        {
                            Utils.RoundPosition(t);
                            prevPosition = t.position;
                        }
                    }
                }
            }
        }

        void SceneGUI(SceneView sceneView)
        {
            e = Event.current;

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
            if (selectedPrefabId != 1)
            {
                currentPos += (Vector3.back * spawnHeight);
                currentPos = Utils.AvoidIntersect(currentPos);
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            if (mouseOverWindow != sceneView)
            {
                Reset();
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
                    spawnHeight += deltaY;
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
                    if (e.button == 0 && selectedPrefabId != 0)
                    {
                        e.Use();
                        Refresh();
                        drawPos = currentPos;
                        Level.CreateAt(GetSelectedPrefab(), Utils.Vec3ToInt(drawPos));
                        Refresh();
                        mouseButtonDown = true;
                        mousePosOnClick = e.mousePosition;
                    }
                    else if (e.button == 1)
                    {
                        selectedPrefabId = 0;
                        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                        if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
                        {
                            for (int i = 0; i < Prefabs.Count; i++)
                            {
                                if (Prefabs[i].transform.name == hit.transform.parent.name)
                                {
                                    selectedPrefabId = i + 2;
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
                            Level.CreateAt(GetSelectedPrefab(), drawPos);
                            Refresh();
                            mousePosOnClick = e.mousePosition;
                        }
                    }
                }
            }

            LevelGizmo.UpdateGizmo(currentPos, gizmoColor);
            LevelGizmo.Enable(selectedPrefabId != 0);
            sceneView.Repaint();
            Repaint();
        }

        Vector3 GetPosition(Vector3 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3 pos = hit.point + (hit.normal * 0.5f);
                if (selectedPrefabId == 1)
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
            if (selectedPrefabId == 1)
            {
                return null;
            }

            return Prefabs[selectedPrefabId - 2];
        }
    }
}

#endif