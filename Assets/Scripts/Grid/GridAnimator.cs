using System;
using System.Collections.Generic;
using GridGame.Blocks;
using GridGame.SO;
using UnityEngine;

namespace GridGame.Grid
{
    public class GridAnimator : MonoBehaviour
    {
        [SerializeField]
        GameLoopEventChannelSO gameLoopEventChannel;

        [SerializeField]
        GridAnimationCollection scheduledMoves;

        [SerializeField]
        float moveTime = 0.18f; // time it takes to move 1 unit

        void Awake()
        {
            scheduledMoves.Clear();
        }

        void OnEnable()
        {
            gameLoopEventChannel.OnMoveStart += PlayScheduledAnimations;
        }

        void OnDisable()
        {
            gameLoopEventChannel.OnMoveStart -= PlayScheduledAnimations;
        }

        void PlayScheduledAnimations()
        {
            if (scheduledMoves.Count == 0)
            {
                gameLoopEventChannel.EndMove();
                return;
            }

            scheduledMoves.ForEach(animation => animation.Play(moveTime, AnimationFinished));
        }

        void AnimationFinished(GridAnimation animation)
        {
            scheduledMoves.Remove(animation);

            if (scheduledMoves.Count == 0)
            {
                gameLoopEventChannel.EndMove();
            }
        }
    }
}