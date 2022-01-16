using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class LevelManager
    {
        public static List<KeyValuePair<string, string>> GetLevelScenes()
        {
            return AssetDatabase.FindAssets("t:Scene")
                .ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new KeyValuePair<string, string>(path, Path.GetFileNameWithoutExtension(path)))
                .Where(level => level.Value.StartsWith("Level_"))
                .ToList();
        }

        public static Level SelectLevel(KeyValuePair<string, string> level)
        {
            var openedScene = EditorSceneManager.OpenScene(level.Key, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(openedScene);

            CloseLevelsExcept(level);

            GameObject levelRoot =
                openedScene.GetRootGameObjects()
                    .ToList()
                    .Find(obj => obj.CompareTag("Level"));

            if (levelRoot == null)
            {
                Debug.LogError("Level scene doesn't contain a Level object");
            }

            return new Level(levelRoot.transform);
        }

        static void CloseLevelsExcept(KeyValuePair<string, string> level)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name.StartsWith("Level_") && scene.name != level.Value)
                {
                    if (scene.isDirty)
                    {
                        if (EditorUtility.DisplayDialog(
                                "Unsaved Changes",
                                $"Do you want to save the changes made to scene <b>{scene.name}</b>?",
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