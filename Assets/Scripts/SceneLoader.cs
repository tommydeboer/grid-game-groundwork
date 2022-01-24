using System;
using System.Collections.Generic;
using System.Linq;
using GridGame.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame
{
    /// <summary>
    /// This class manages the scenes loading and unloading
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField]
        SceneField initializationScene;

        [SerializeField]
        SceneField openingLevel;

        //The load event we are listening to
        [Header("Load Event")]
        [SerializeField]
        LoadEventChannelSO loadEventChannel;

        //List of the scenes to load and track progress
        readonly List<AsyncOperation> scenesToLoadAsyncOperations = new();

        //List of scenes to unload
        readonly List<Scene> scenesToUnload = new();

        //Keep track of the scene we want to set as active (for lighting/skybox)
        SceneField activeScene;

        void OnEnable()
        {
            loadEventChannel.OnLoadingRequested += LoadScene;
        }

        void OnDisable()
        {
            loadEventChannel.OnLoadingRequested -= LoadScene;
        }

        void Start()
        {
            if (!SceneManager.GetActiveScene().name.StartsWith("Level_"))
            {
                LoadScene(openingLevel, false);
            }
        }

        void LoadScene(SceneField sceneToLoad, bool showLoadingScreen)
        {
            AddScenesToUnload();

            activeScene = sceneToLoad;

            string sceneName = sceneToLoad.SceneName;
            if (!IsLoaded(sceneName))
            {
                scenesToLoadAsyncOperations.Add(SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive));
            }

            scenesToLoadAsyncOperations[0].completed += SetActiveScene;
            scenesToLoadAsyncOperations.Clear();

            UnloadScenes();
        }

        void SetActiveScene(AsyncOperation asyncOp)
        {
            Scene scene = SceneManager.GetSceneByName(activeScene.SceneName);
            GameObject levelRoot = scene.GetRootGameObjects().First(go => go.CompareTag("Level"));
            SceneManager.SetActiveScene(scene);

            if (scene.IsLevel())
            {
                loadEventChannel.OnLevelLoaded(scene);
            }

            // TODO activate when undo system is back online
            // State.Init();
        }

        void AddScenesToUnload()
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != initializationScene.SceneName)
                {
                    scenesToUnload.Add(scene);
                }
            }
        }

        void UnloadScenes()
        {
            foreach (Scene scene in scenesToUnload)
            {
                SceneManager.UnloadSceneAsync(scene);
            }

            scenesToUnload.Clear();
        }

        static bool IsLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}