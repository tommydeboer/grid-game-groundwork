using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;

namespace GridGame.Editor
{
    public static class EditorAssets
    {
        public static EditorPrefabs EditorPrefabs;
        public static State State;
        public static GameObject SelectionOverlay;
        public static SceneTemplateAsset LevelTemplate;

        public const string ScenesLocation = "Assets/Scenes/";

        const string EditorPrefabsLocation = "Assets/Resources/Editor/EditorPrefabs.asset";
        const string EditorStateLocation = "Assets/Resources/Editor/State.asset";
        const string SelectionOverlayLocation = "Assets/Resources/Editor/SelectionOverlay.prefab";
        const string LevelTemplateLocation = "Assets/Resources/Editor/LevelTemplate.scenetemplate";

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            LoadEditorPrefabs();
            LoadState();
            LoadSelectionOverlay();
            LoadLevelTemplate();
            AddTag("Level");
            AddTag("Tile");
        }

        static void LoadState()
        {
            if (!State)
            {
                State = AssetDatabase.LoadAssetAtPath<State>(EditorStateLocation);
                if (!State)
                {
                    State = ScriptableObject.CreateInstance<State>();
                    AssetDatabase.CreateAsset(State, EditorStateLocation);
                    AssetDatabase.Refresh();
                }
            }

            State.EditorPrefabs = EditorPrefabs;
        }

        static void LoadEditorPrefabs()
        {
            if (!EditorPrefabs)
            {
                EditorPrefabs = AssetDatabase.LoadAssetAtPath<EditorPrefabs>(EditorPrefabsLocation);
                if (!EditorPrefabs)
                {
                    EditorPrefabs = ScriptableObject.CreateInstance<EditorPrefabs>();
                    AssetDatabase.CreateAsset(EditorPrefabs, EditorPrefabsLocation);
                    AssetDatabase.Refresh();
                }
            }
        }

        static void LoadSelectionOverlay()
        {
            if (!SelectionOverlay)
            {
                SelectionOverlay = AssetDatabase.LoadAssetAtPath<GameObject>(SelectionOverlayLocation);

                if (!SelectionOverlay)
                {
                    Debug.LogWarning("Selection Overlay asset is missing");
                }
            }
        }

        static void LoadLevelTemplate()
        {
            if (!LevelTemplate)
            {
                LevelTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(LevelTemplateLocation);

                if (!LevelTemplate)
                {
                    Debug.LogWarning("Level template is missing");
                }
            }
        }

        static void AddTag(string name)
        {
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

            if (asset is { Length: > 0 })
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");

                for (int i = 0; i < tags.arraySize; i++)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == name) return;
                }

                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = name;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }
    }
}