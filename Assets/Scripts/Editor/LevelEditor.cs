#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    //TODO add play mode persistence
    public class LevelEditor : EditorWindow
    {
        #region CONFIG VARIABLES

        bool showConfiguration;
        bool showPlacement = true;
        bool showLevelSelection;
        GUIStyle margin;

        readonly string[] rotateLabels =
        {
            "0", "90", "180", "270"
        };

        readonly string[] modeLabels = Enum.GetNames(typeof(PlacementMode));

        #endregion

        #region GUI STATE

        int rotateInt;
        int modeInt;
        int sceneLevelIndex;
        readonly List<string> sceneLevels = new();

        #endregion

        #region EDITOR CLASSES

        static State state;
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

            if (!state)
            {
                state =
                    AssetDatabase.LoadAssetAtPath<State>("Assets/Resources/LevelEditorValues.asset");
                if (!state)
                {
                    state = CreateInstance<State>();
                    AssetDatabase.CreateAsset(state, "Assets/Resources/LevelEditorValues.asset");
                    AssetDatabase.Refresh();
                }
            }

            state.EditorPrefabs = editorPrefabs;
        }

        void OnEnable()
        {
            sceneViewInteraction = new SceneViewInteraction(this, state);
            SceneView.duringSceneGui += sceneViewInteraction.OnSceneGUI;

            margin = new GUIStyle {margin = new RectOffset(15, 15, 10, 15)};

            CreateGizmoObject();
            EnsureTagsExist();

            if (string.IsNullOrEmpty(state.CurrentLevel))
            {
                GameObject level = GameObject.FindGameObjectWithTag("Level");
                if (level != null)
                {
                    state.CurrentLevel = level.name;
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
                state.GizmoColor = EditorGUILayout.ColorField("Gizmo Color:", state.GizmoColor);

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
                state.PlacementMode = (PlacementMode) GUILayout.Toolbar((int) state.PlacementMode, modeLabels);
                GUI.enabled = state.PlacementMode == PlacementMode.Create;

                BigSpace();

                GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
                IEnumerable<string> labels = state.Prefabs.Select(
                    (prefab, i) => prefab.transform.name + $" ({i + 1})");
                state.SelectedPrefabId = GUILayout.SelectionGrid(state.SelectedPrefabId, labels.ToArray(), 1);

                BigSpace();

                GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);
                rotateInt = GUILayout.SelectionGrid(rotateInt, rotateLabels, 4);
                SetRotation();

                BigSpace();

                state.SpawnHeight = EditorGUILayout.IntSlider("Spawn at height:", state.SpawnHeight, 0, 20);

                GUI.enabled = true;
            }
        }

        void SetRotation()
        {
            state.SpawnRotation = new Vector3(0, rotateInt * 90, 0);
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
                    if (sceneLevels[i] == state.CurrentLevel)
                    {
                        sceneLevelIndex = i;
                    }
                }

                sceneLevelIndex = EditorGUILayout.Popup(sceneLevelIndex, sceneLevels.ToArray());
                state.CurrentLevel = sceneLevels[sceneLevelIndex];
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