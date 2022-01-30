using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GridGame.Events
{
    /// <summary>
    /// This class is a used for scene loading events.
    /// Takes an array of the scenes we want to load and a bool to specify if we want to show a loading screen.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Load Event Channel")]
    public class LoadEventChannelSO : ScriptableObject
    {
        public UnityAction<SceneReference, bool> OnLoadingRequested;
        public UnityAction<Scene> OnLevelLoaded;

        public void RaiseSceneLoadRequestEvent(SceneReference scene, bool showLoadingScreen)
        {
            if (OnLoadingRequested != null)
            {
                OnLoadingRequested.Invoke(scene, showLoadingScreen);
            }
            else
            {
                Debug.LogWarning("A Scene loading was requested, but nobody picked it up." +
                                 "Check why there is no LevelLoader already present, " +
                                 "and make sure it's listening on this Load Event channel.");
            }
        }

        public void RaiseLevelLoadedEvent(Scene scene)
        {
            OnLevelLoaded?.Invoke(scene);
        }
    }
}