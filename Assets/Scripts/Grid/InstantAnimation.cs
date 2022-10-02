using System;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.Player;
using UnityEngine;

namespace GridGame.Grid
{
    /// <summary>
    /// Instantly swaps grid positions at the end of an animation. 
    /// </summary>
    internal record InstantAnimation : GridAnimation
    {
        public Vector3 TargetPosition { private get; init; }
        Action<GridAnimation> animationFinished { get; set; }

        public override void Play(float moveTime, Action<GridAnimation> animationFinished)
        {
            this.animationFinished = animationFinished;
        }

        public void AnimationCallback()
        {
            Movable.transform.position = TargetPosition;
            animationFinished(this);
        }
    }
}