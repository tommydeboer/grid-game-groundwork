using System;
using System.Collections.Generic;
using System.Linq;
using GridGame.SO;
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
        SceneReference initializationScene;

        [SerializeField]
        SceneReference openingLevel;

        //The load event we are listening to
        [Header("Load Event")]
        [SerializeField]
        LoadEventChannelSO loadEventChannel;

        //List of the scenes to load and track progress
        readonly List<AsyncOperation> scenesToLoadAsyncOperations = new();

        //List of scenes to unload
        readonly List<Scene> scenesToUnload = new();

        //Keep track of the scene we want to set as active (for lighting/skybox)
        SceneReference activeScene;

        void Awake()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsLevel())
                {
                    loadEventChannel.RaiseLevelLoadedEvent(scene);
                    break;
                }
            }

            if (!SceneManager.GetActiveScene().IsLevel())
            {
                LoadScene(openingLevel, false);
            }
        }

        void OnEnable()
        {
            loadEventChannel.OnLoadingRequested += LoadScene;
        }

        void OnDisable()
        {
            loadEventChannel.OnLoadingRequested -= LoadScene;
        }


        void LoadScene(SceneReference sceneToLoad, bool showLoadingScreen)
        {
            if (IsLoaded(sceneToLoad.ScenePath))
            {
                return;
            }

            AddScenesToUnload();

            activeScene = sceneToLoad;

            scenesToLoadAsyncOperations.Add(SceneManager.LoadSceneAsync(sceneToLoad.ScenePath, LoadSceneMode.Additive));
            scenesToLoadAsyncOperations[0].completed += SetActiveScene;
            scenesToLoadAsyncOperations.Clear();

            UnloadScenes();
        }

        void SetActiveScene(AsyncOperation asyncOp)
        {
            Scene scene = SceneManager.GetSceneByPath(activeScene.ScenePath);
            SceneManager.SetActiveScene(scene);

            if (scene.IsLevel())
            {
                loadEventChannel.OnLevelLoaded(scene);
            }
        }

        void AddScenesToUnload()
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path != initializationScene.ScenePath)
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

        static bool IsLoaded(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path == scenePath)
                {
                    return true;
                }
            }

            return false;
        }
    }
}