using System;
using DG.Tweening;
using UnityEngine;

namespace GridGame.Grid
{
    internal record LinearAnimation : GridAnimation
    {
        public Vector3 TargetPosition { private get; init; }

        public override void Play(float moveTime, Action<GridAnimation> animationFinished)
        {
            Movable.transform
                .DOMove(TargetPosition, moveTime)
                .OnComplete(() => animationFinished(this))
                .SetEase(Ease.Linear);
        }
    }
}