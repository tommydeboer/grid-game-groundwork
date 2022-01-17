using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;

namespace Editor
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

            if (!State)
            {
                State =
                    AssetDatabase.LoadAssetAtPath<State>(EditorStateLocation);
                if (!State)
                {
                    State = ScriptableObject.CreateInstance<State>();
                    AssetDatabase.CreateAsset(State, EditorStateLocation);
                    AssetDatabase.Refresh();
                }
            }

            State.EditorPrefabs = EditorPrefabs;

            if (!SelectionOverlay)
            {
                SelectionOverlay =
                    AssetDatabase.LoadAssetAtPath<GameObject>(SelectionOverlayLocation);

                if (!SelectionOverlay)
                {
                    Debug.LogWarning("Selection Overlay asset is missing");
                }
            }

            if (!LevelTemplate)
            {
                LevelTemplate = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(LevelTemplateLocation);

                if (!LevelTemplate)
                {
                    Debug.LogWarning("Level template is missing");
                }
            }
        }
    }
}