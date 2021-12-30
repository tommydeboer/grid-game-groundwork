using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class EditorAssets
    {
        public static EditorPrefabs EditorPrefabs;
        public static State State;
        public static GameObject SelectionOverlay;

        const string EditorPrefabsLocation = "Assets/Resources/Editor/EditorPrefabs.asset";
        const string EditorStateLocation = "Assets/Resources/Editor/State.asset";
        const string SelectionOverlayLocation = "Assets/Resources/Editor/SelectionOverlay.prefab";

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
        }
    }
}