using System;
using GridGame.Player;
using GridGame.SO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GridGame.Blocks
{
    [RequireComponent(typeof(Triggerable))]
    public class Exit : EntityBehaviour
    {
        [Header("Levels")]
        [SerializeField]
        [CanBeNull]
        SceneList levelList;

        [Header("Level Override")]
        [SerializeField]
        [CanBeNull]
        SceneReference nextLevel;

        [Header("Channel")]
        [SerializeField]
        LoadEventChannelSO loadChannel;


        public void Check()
        {
            if (Entity.Intersects<Hero>())
            {
                GoToNextLevel();
            }
        }

        void GoToNextLevel()
        {
            if (nextLevel != null && !string.IsNullOrEmpty(nextLevel.ScenePath))
            {
                loadChannel.RequestSceneLoad(nextLevel, false);
            }
            else if (levelList != null)
            {
                var currentLevel = SceneManager.GetActiveScene();
                loadChannel.RequestSceneLoad(levelList.Next(currentLevel), false);
            }
        }

        void OnValidate()
        {
            Debug.Assert(nextLevel != null || levelList != null,
                "One of 'level list' or 'next level' must be set",
                gameObject);
            Debug.Assert(loadChannel != null, "Exit has no channel to broadcast to", gameObject);
        }
    }
}