using System;
using System.Collections.Generic;
using System.Linq;
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
        GridAnimationCollection scheduledAnimations;

        [SerializeField]
        MovableCollection allMovables;

        [SerializeField]
        float moveTime = 0.18f; // time it takes to move 1 unit

        [SerializeField]
        float fallTime = 0.1f; // time it takes to fall 1 unit

        public bool IsAnimating { get; private set; }

        void Awake()
        {
            scheduledAnimations.Clear();
        }

        void OnEnable()
        {
            gameLoopEventChannel.OnMoveStart += PlayScheduledAnimations;
            gameLoopEventChannel.OnFallStart += ExecuteGravity;
        }

        void OnDisable()
        {
            gameLoopEventChannel.OnMoveStart -= PlayScheduledAnimations;
            gameLoopEventChannel.OnFallStart -= ExecuteGravity;
        }

        void PlayScheduledAnimations()
        {
            if (scheduledAnimations.Count == 0)
            {
                IsAnimating = false;
                gameLoopEventChannel.EndMove();
                return;
            }

            IsAnimating = true;
            scheduledAnimations.ForEach(animation => animation.Play(moveTime, MoveAnimationCallback));
        }

        void ExecuteGravity()
        {
            allMovables.OrderBy(m => m.transform.position.y).ToList().ForEach(m => m.Fall());

            if (scheduledAnimations.Count == 0)
            {
                IsAnimating = false;
                gameLoopEventChannel.EndFall();
                return;
            }

            IsAnimating = true;
            scheduledAnimations.ForEach(animation => animation.Play(fallTime, FallAnimationCallback));
        }

        void MoveAnimationCallback(GridAnimation animation)
        {
            scheduledAnimations.Remove(animation);

            if (scheduledAnimations.Count == 0)
            {
                gameLoopEventChannel.EndMove();
            }
        }

        void FallAnimationCallback(GridAnimation animation)
        {
            scheduledAnimations.Remove(animation);

            if (scheduledAnimations.Count == 0)
            {
                ExecuteGravity();
            }
        }
    }
}