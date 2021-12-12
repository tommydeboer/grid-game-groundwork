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
        #region CONFIG VARIABLES

        bool showConfiguration;
        bool showPlacement = true;
        bool showLevelSelection;
        GUIStyle margin;

        readonly string[] rotateStrings =
        {
            "0", "90", "180", "270"
        };

        #endregion

        #region GUI STATE

        int rotateInt;
        int selectionInt;
        int sceneLevelIndex;
        readonly List<string> sceneLevels = new();

        #endregion

        #region EDITOR CLASSES

        static LevelEditorValues values;
        static EditorPrefabs editorPrefabs;
        SceneViewInteraction sceneViewInteraction;

        #endregion

        #region INITIALISATION

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow(typeof(LevelEditor));
            var texture = Resources.Load<Texture2D>("ggg");
            editorWindow.titleContent = new GUIContent("Level Editor", texture);
        }

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            if (!editorPrefabs)
            {
                editorPrefabs = AssetDatabase.LoadAssetAtPath<EditorPrefabs>("Assets/Resources/EditorPrefabs.asset");
                if (!editorPrefabs)
                {
                    editorPrefabs = CreateInstance<EditorPrefabs>();
                    AssetDatabase.CreateAsset(editorPrefabs, "Assets/Resources/EditorPrefabs.asset");
                    AssetDatabase.Refresh();
                }
            }

            if (!values)
            {
                values =
                    AssetDatabase.LoadAssetAtPath<LevelEditorValues>("Assets/Resources/LevelEditorValues.asset");
                if (!values)
                {
                    values = CreateInstance<LevelEditorValues>();
                    AssetDatabase.CreateAsset(values, "Assets/Resources/LevelEditorValues.asset");
                    AssetDatabase.Refresh();
                }
            }

            values.EditorPrefabs = editorPrefabs;
        }

        void OnEnable()
        {
            sceneViewInteraction = new SceneViewInteraction(this, values);
            SceneView.duringSceneGui += sceneViewInteraction.OnSceneGUI;

            margin = new GUIStyle {margin = new RectOffset(15, 15, 10, 15)};

            CreateGizmoObject();
            EnsureTagsExist();

            if (string.IsNullOrEmpty(values.CurrentLevel))
            {
                GameObject level = GameObject.FindGameObjectWithTag("Level");
                if (level != null)
                {
                    values.CurrentLevel = level.name;
                }
            }
        }

        static void EnsureTagsExist()
        {
            TagHelper.AddTag("Level");
            TagHelper.AddTag("Tile");
        }

        static void CreateGizmoObject()
        {
            LevelGizmo levelGizmo = FindObjectOfType<LevelGizmo>();
            if (levelGizmo == null)
            {
                new GameObject("LevelGizmo").AddComponent<LevelGizmo>();
            }
        }

        #endregion

        #region UI

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
                values.GizmoColor = EditorGUILayout.ColorField("Gizmo Color:", values.GizmoColor);

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
                labels.AddRange(from prefab in values.Prefabs select prefab.transform.name);

                GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
                selectionInt = GUILayout.SelectionGrid(selectionInt, labels.ToArray(), 1);
                SetSelection();

                BigSpace();

                GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);
                rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4);
                SetRotation();

                BigSpace();

                values.SpawnHeight = EditorGUILayout.IntSlider("Spawn at height:", values.SpawnHeight, 0, 20);
            }
        }

        void SetSelection()
        {
            values.PlacementMode = selectionInt > 2 ? PlacementMode.CREATE : (PlacementMode) selectionInt;
            if (selectionInt > 1)
            {
                values.SetPrefab(selectionInt - 2);
            }
        }

        void SetRotation()
        {
            values.SpawnRotation = new Vector3(0, rotateInt * 90, 0);
        }

        void DrawLevelSelectionUI()
        {
            // TODO fix: new levels not added to dropdown
            using (new GUILayout.VerticalScope(margin))
            {
                GUILayout.Label("Currently Editing: ", EditorStyles.boldLabel);

                sceneLevelIndex = 0;
                for (int i = 0; i < sceneLevels.Count; i++)
                {
                    if (sceneLevels[i] == values.CurrentLevel)
                    {
                        sceneLevelIndex = i;
                    }
                }

                sceneLevelIndex = EditorGUILayout.Popup(sceneLevelIndex, sceneLevels.ToArray());
                values.CurrentLevel = sceneLevels[sceneLevelIndex];
            }
        }

        static void BigSpace()
        {
            EditorGUILayout.Space(15);
        }

        #endregion

        public void Refresh()
        {
            Game game = FindObjectOfType<Game>();
            if (game != null)
            {
                game.EditorRefresh();
            }

            RefreshSceneLevels();
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
    }
}

#endif