#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    //TODO add play mode persistence
    public class LevelEditor : EditorWindow
    {
        #region CONFIG VARIABLES

        GUIStyle margin;

        readonly string[] rotateLabels =
        {
            "0", "90", "180", "270"
        };

        readonly string[] modeLabels = Enum.GetNames(typeof(Mode));
        Texture refreshIcon;
        Texture newIcon;

        #endregion

        #region GUI STATE

        bool showConfiguration;
        bool showPlacement = true;
        bool showLevelSelection;
        Mode mode;
        int selectedPrefabId;
        int spawnHeight;
        int rotateInt;
        int modeInt;
        int sceneLevelIndex;
        List<KeyValuePair<string, string>> levels;

        #endregion

        #region EDITOR CLASSES

        State state;
        EditorPrefabs editorPrefabs;

        #endregion

        #region INITIALISATION

        [MenuItem("Window/Level Editor")]
        public static LevelEditor ShowWindow()
        {
            LevelEditor levelEditor = GetWindow<LevelEditor>();
            levelEditor.refreshIcon = EditorGUIUtility.IconContent("TreeEditor.Refresh").image;
            levelEditor.newIcon = EditorGUIUtility.IconContent("Toolbar Plus").image;
            var texture = EditorGUIUtility.IconContent("PreMatCube").image;
            levelEditor.titleContent = new GUIContent("Level Editor", texture);
            return levelEditor;
        }

        void OnEnable()
        {
            state = EditorAssets.State;
            editorPrefabs = EditorAssets.EditorPrefabs;
            margin = new GUIStyle { margin = new RectOffset(15, 15, 10, 15) };
            LevelManager.onLevelCreate += OnLevelCreate;
            RefreshLevelList();
        }

        void OnDisable()
        {
            LevelManager.onLevelCreate -= OnLevelCreate;
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
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    SerializedObject serialObj = new SerializedObject(editorPrefabs);
                    SerializedProperty serialProp = serialObj.FindProperty("prefabs");
                    EditorGUILayout.PropertyField(serialProp, false);

                    if (check.changed)
                    {
                        serialObj.ApplyModifiedProperties();
                    }
                }
            }
        }

        void DrawPlacementUI()
        {
            using (new GUILayout.VerticalScope(margin))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    mode = (Mode)GUILayout.Toolbar((int)state.Mode, modeLabels);
                    if (check.changed)
                    {
                        state.Mode = mode;
                    }
                }

                GUI.enabled = state.Mode == Mode.Create;

                BigSpace();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.Label("Selected GameObject:", EditorStyles.boldLabel);
                    IEnumerable<string> labels = state.Prefabs.Select(
                        (prefab, i) => prefab.transform.name + $" ({i + 1})");
                    selectedPrefabId = GUILayout.SelectionGrid(state.SelectedPrefabId, labels.ToArray(), 1);

                    if (check.changed)
                    {
                        state.SelectedPrefabId = selectedPrefabId;
                    }
                }

                BigSpace();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.Label("GameObject Rotation:", EditorStyles.boldLabel);

                    rotateInt = GUILayout.SelectionGrid((int)state.SpawnRotation.y / 90, rotateLabels, 4);

                    if (check.changed)
                    {
                        state.SpawnRotation = new Vector3(0, rotateInt * 90, 0);
                    }
                }

                BigSpace();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    spawnHeight = EditorGUILayout.IntSlider("Spawn at height:", state.SpawnHeight, 0,
                        State.MAX_SPAWN_HEIGHT);

                    if (check.changed)
                    {
                        state.SpawnHeight = spawnHeight;
                    }
                }

                GUI.enabled = true;
            }
        }

        void DrawLevelSelectionUI()
        {
            using (new GUILayout.VerticalScope(margin))
            {
                GUILayout.Label("Currently Editing: ", EditorStyles.boldLabel);
                using (new GUILayout.HorizontalScope())
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        sceneLevelIndex =
                            EditorGUILayout.Popup(sceneLevelIndex, levels.Select(level => level.Value).ToArray());

                        if (check.changed)
                        {
                            state.CurrentLevel = LevelManager.SelectLevel(levels[sceneLevelIndex]);
                        }

                        if (GUILayout.Button(newIcon, GUILayout.Width(30)))
                        {
                            InputDialog.ShowDialog(
                                "Enter the name of the level",
                                "Create",
                                LevelManager.CreateNewLevel,
                                "Level_"
                            );
                        }

                        if (GUILayout.Button(refreshIcon, GUILayout.Width(30)))
                        {
                            RefreshLevelList();
                        }
                    }
                }
            }
        }

        static void BigSpace()
        {
            EditorGUILayout.Space(15);
        }

        #endregion

        void OnLevelCreate(Level level)
        {
            RefreshLevelList();

            // set level dropdown to new level
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].Value == level.Scene.name)
                {
                    sceneLevelIndex = i;
                }
            }

            state.CurrentLevel = level;
        }

        void RefreshLevelList()
        {
            levels = LevelManager.GetLevelScenes();
        }

        public static void Refresh()
        {
            Game game = FindObjectOfType<Game>();
            if (game != null)
            {
                game.EditorRefresh();
            }
        }

        [Shortcut("Enter Play Mode", null, KeyCode.P)]
        static void PlayShortcut()
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");

            // set focus to Game window
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}

#endif