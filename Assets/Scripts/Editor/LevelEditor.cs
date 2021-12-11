#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    //TODO add play mode persistence: https://stackoverflow.com/questions/56594340/store-editor-values-between-editor-sessions
    public class LevelEditor : EditorWindow
    {
        public int SelectedPrefabId { get; set; }
        public int SpawnHeight { get; set; }
        public Color GizmoColor { get; set; } = Color.white;

        int rotateInt;

        readonly string[] rotateStrings =
        {
            "0", "90", "180", "270"
        };

        string currentLevel;

        bool showConfiguration;
        bool showPlacement = true;
        bool showLevelSelection;
        GUIStyle margin;
        SceneViewInteraction sceneViewInteraction;

        int sceneLevelIndex;

        static EditorPrefabs editorPrefabs;
        public static List<GameObject> Prefabs => editorPrefabs.Prefabs;

        public Level Level
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
            sceneViewInteraction = new SceneViewInteraction(this);
            SceneView.duringSceneGui += sceneViewInteraction.OnSceneGUI;

            margin = new GUIStyle {margin = new RectOffset(15, 15, 10, 15)};

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

        public void Reset()
        {
            CreateGizmoObject();
        }

        public void Refresh()
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
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.VerticalScope())
                {
                    showLevelSelection = EditorGUILayout.Foldout(showLevelSelection, "Level");
                    if (showLevelSelection) DrawLevelSelectionUI();
                }
            }

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.VerticalScope())
                {
                    showPlacement = EditorGUILayout.Foldout(showPlacement, "Placement");
                    if (showPlacement) DrawPlacementUI();
                }
            }

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.VerticalScope())
                {
                    showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration");
                    if (showConfiguration) DrawConfigurationUI();
                }
            }
        }

        void DrawConfigurationUI()
        {
            using (new GUILayout.VerticalScope(margin))
            {
                GizmoColor = EditorGUILayout.ColorField("Gizmo Color:", GizmoColor);

                BigSpace();

                SerializedObject serialObj = new SerializedObject(editorPrefabs);
                SerializedProperty serialProp = serialObj.FindProperty("prefabs");
                EditorGUILayout.PropertyField(serialProp, true);
                serialObj.ApplyModifiedProperties();
            }
        }

        void DrawPlacementUI()
        {
            using (new GUILayout.VerticalScope(margin))
            {
                var labels = new List<string> {"None", "Erase"};
                labels.AddRange(from prefab in Prefabs select prefab.transform.name);

                GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
                SelectedPrefabId = GUILayout.SelectionGrid(SelectedPrefabId, labels.ToArray(), 1);

                BigSpace();

                GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);
                rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4);

                BigSpace();

                SpawnHeight = EditorGUILayout.IntSlider("Spawn at height:", SpawnHeight, 0, 20);
            }
        }

        void DrawLevelSelectionUI()
        {
            using (new GUILayout.VerticalScope(margin))
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
            }
        }

        static void BigSpace()
        {
            EditorGUILayout.Space(15);
        }
    }
}

#endif