using UnityEditor;
using UnityEngine.SceneManagement;

namespace Events
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// This class is a used for scene loading events.
    /// Takes an array of the scenes we want to load and a bool to specify if we want to show a loading screen.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Load Event Channel")]
    public class LoadEventChannelSO : ScriptableObject
    {
        public UnityAction<SceneField, bool> OnLoadingRequested;
        public UnityAction<Scene> OnLoadingFinished;

        public void RaiseSceneLoadRequestEvent(SceneField scene, bool showLoadingScreen)
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

        public void RaiseSceneLoadedEvent(Scene scene)
        {
            OnLoadingFinished?.Invoke(scene);
        }
    }
}