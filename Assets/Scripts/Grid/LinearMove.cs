using System;
using DG.Tweening;
using UnityEngine;

namespace GridGame.Grid
{
    internal record LinearMove : GridAnimation
    {
        public Vector3 targetPosition { private get; init; }

        public override void Play(float moveTime, Action<GridAnimation> animationFinished)
        {
            Movable.transform
                .DOMove(targetPosition, moveTime)
                .OnComplete(() => animationFinished(this))
                .SetEase(Ease.Linear);
        }
    }
}