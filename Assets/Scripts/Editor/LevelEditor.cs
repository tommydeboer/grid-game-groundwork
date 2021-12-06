#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelEditor : EditorWindow
    {
        int selectedPrefabId;
        string[] selectStrings;

        int rotateInt;

        readonly string[] rotateStrings =
        {
            "0", "90", "180", "270"
        };

        int spawnHeight;
        string currentLevel;
        bool overwriteLevel;

        public GameObject[] prefabs;

        bool isHoldingAlt;
        bool mouseButtonDown;
        Vector3 drawPos;
        static bool playModeActive;
        Event e;
        bool titleIsSet;

        static string textFilePath => Application.dataPath + "/leveleditorprefabs.txt";

        int sceneLevelIndex;
        bool snapToGrid = true;
        bool isLoading;
        Vector3 prevPosition;
        Vector2 scrollPos;
        Color gizmoColor = Color.white;
        Vector2 mousePosOnClick;

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

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(LevelEditor));
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += SceneGUI;
            EditorApplication.playModeStateChanged += ChangedPlayModeState;
        }

        void ChangedPlayModeState(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    playModeActive = true;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    playModeActive = false;
                    GetPlayModeJobs();
                    break;
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

        void PopulateList()
        {
            if (prefabs.Length == 0 && File.Exists(textFilePath))
            {
                string[] prefabNames = File.ReadAllLines(textFilePath);

                prefabs = prefabNames
                    .Select(Resources.Load<GameObject>)
                    .Where(go => go != null).ToArray();
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
            string previousLevel = currentLevel;

            if (!titleIsSet)
            {
                titleIsSet = true;
                var texture = Resources.Load<Texture2D>("ggg");
                titleContent = new GUIContent("Level Editor", texture);
            }

            GUI.backgroundColor = Color.grey;

            BeginWindows();
            Rect windowRect = new Rect(20, 20, 420, 650);

            GUIStyle myStyle = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(15, 15, 15, 15)
            };

            GUILayout.Window(1, windowRect, GetWindows, "", myStyle);
            EndWindows();

            if (previousLevel != currentLevel)
            {
                Selection.activeGameObject = Level.Root.gameObject;
            }
        }

        void GetWindows(int unusedWindowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            RefreshSceneLevels();
            if (sceneLevels.Count > 0)
            {
                DrawingWindow();
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawingWindow()
        {
            GUILayout.Label("DRAWING", EditorStyles.centeredGreyMiniLabel);

            BigSpace();

            if (string.IsNullOrEmpty(currentLevel))
            {
                GameObject level = GameObject.FindGameObjectWithTag("Level");
                if (level != null)
                {
                    currentLevel = level.name;
                }
            }

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
                return;
            }

            BigSpace();

            if (prefabs != null && prefabs.Length > 0)
            {
                var selectStringsTmp = new List<string> {"None", "Erase"};
                selectStringsTmp.AddRange(
                    from prefab in prefabs where prefab != null select prefab.transform.name);

                selectStrings = selectStringsTmp.ToArray();
            }
            else
            {
                PopulateList();
                return;
            }

            GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
            selectedPrefabId = GUILayout.SelectionGrid(selectedPrefabId, selectStrings, 3, GUILayout.Width(370));

            BigSpace();

            GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);
            rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4, GUILayout.Width(330));

            BigSpace();

            gizmoColor = EditorGUILayout.ColorField("Gizmo Color:", gizmoColor);

            ///////////////// SPAWN //////////////////

            spawnHeight = EditorGUILayout.IntSlider("Spawn at height:", spawnHeight, 0, 20);

            snapToGrid = EditorGUILayout.Toggle("Snap to grid:", snapToGrid);

            BigSpace();
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

        void GetPlayModeJobs()
        {
            IEnumerable<LevelPlayModePersistence.Job> jobs = LevelPlayModePersistence.GetJobs();
            foreach (LevelPlayModePersistence.Job job in jobs)
            {
                if (job.name == "clear")
                {
                    Level.ClearAt(Utils.Vec3ToInt(job.position));
                }
                else
                {
                    PlayModeCreateObject(job.name, job.position, job.eulerAngles);
                }
            }
        }

        void PlayModeCreateObject(string objName, Vector3 position, Vector3 eulerAngles)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i].transform.name == objName)
                {
                    selectedPrefabId = i + 2;
                }
            }

            Level.CreateAt(GetSelectedPrefab(), position, eulerAngles);
            Refresh();
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
                            for (int i = 0; i < prefabs.Length; i++)
                            {
                                if (prefabs[i].transform.name == hit.transform.parent.name)
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

            return prefabs[selectedPrefabId - 2];
        }
    }
}

#endif