using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;

namespace GridGame.Editor
{
    public static class LevelManager
    {
        public delegate void OnLevelCreate(Level level);

        public static event OnLevelCreate onLevelCreate;

        public static List<KeyValuePair<string, string>> GetLevelScenes()
        {
            return AssetDatabase.FindAssets("t:Scene")
                .ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new KeyValuePair<string, string>(path, Path.GetFileNameWithoutExtension(path)))
                .Where(level => level.Value.StartsWith("Level_"))
                .ToList();
        }


        public static void CreateNewLevel(string name)
        {
            var result = SceneTemplateService.Instantiate(EditorAssets.LevelTemplate, true,
                EditorAssets.ScenesLocation + $"/{name}.unity");
            Scene scene = result.scene;
            Level level = ChangeLevel(scene);
            onLevelCreate?.Invoke(level);
        }

        public static Level SelectLevel(KeyValuePair<string, string> level)
        {
            var scene = EditorSceneManager.OpenScene(level.Key, OpenSceneMode.Additive);
            return ChangeLevel(scene);
        }

        public static Level GetLevel(Scene scene)
        {
            return new Level(scene.name);
        }

        static Level ChangeLevel(Scene scene)
        {
            SceneManager.SetActiveScene(scene);
            CloseLevelsExcept(scene);
            return GetLevel(scene);
        }

        public static void CloseLevelsExcept(Scene excludedScene)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsLevel() && scene.name != excludedScene.name)
                {
                    if (scene.isDirty)
                    {
                        if (EditorUtility.DisplayDialog(
                                "Unsaved Changes",
                                $"Do you want to save the changes made to scene {scene.name}?",
                                "Yes", "No"))
                        {
                            EditorSceneManager.SaveScene(scene);
                        }
                    }

#pragma warning disable 0618
                    // TODO change to UnloadSceneAsync
                    SceneManager.UnloadScene(scene);
#pragma warning restore 0618
                }
            }
        }
    }
}