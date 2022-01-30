using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Scene List")]
    public class SceneList : ScriptableObject
    {
        [SerializeField]
        List<SceneReference> scenes;

        public SceneReference Next(Scene currentScene)
        {
            int index = scenes.FindIndex(scene => scene.ScenePath.Equals(currentScene.path));
            return index < scenes.Count ? scenes[index + 1] : scenes[0];
        }

        [CanBeNull]
        public SceneReference GetAt(int index)
        {
            return index < scenes.Count ? scenes[index] : null;
        }
    }
}