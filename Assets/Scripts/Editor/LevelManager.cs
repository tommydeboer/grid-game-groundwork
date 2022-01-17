using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
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

        static Level ChangeLevel(Scene scene)
        {
            SceneManager.SetActiveScene(scene);

            CloseLevelsExcept(scene.name);

            GameObject levelRoot =
                scene.GetRootGameObjects()
                    .ToList()
                    .Find(obj => obj.CompareTag("Level"));

            if (levelRoot == null)
            {
                Debug.LogError("Level scene doesn't contain a Level object");
            }

            return new Level(levelRoot.transform, scene);
        }

        static void CloseLevelsExcept(string name)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name.StartsWith("Level_") && scene.name != name)
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