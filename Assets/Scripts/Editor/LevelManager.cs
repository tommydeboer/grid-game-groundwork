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
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name.StartsWith("Level_") && scene.name != level.Value)
                {
                    EditorSceneManager.SaveScene(scene);
                    SceneManager.UnloadSceneAsync(scene);
                }
            }

            GameObject levelRoot =
                openedScene.GetRootGameObjects()
                    .ToList()
                    .Find(obj => obj.CompareTag("Level"));

            return new Level(levelRoot.transform);
        }
    }
}