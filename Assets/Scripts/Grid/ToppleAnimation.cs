using System;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GridGame.Grid
{
    public record ToppleAnimation : GridAnimation
    {
        public Vector3 TargetPosition { get; init; }

        public override void Play(float moveTime, Action<GridAnimation> finishCallback)
        {
            var transform = Movable.transform;
            var position = transform.position;
            var direction = (TargetPosition - position).normalized;

            var pivot = new GameObject
            {
                transform =
                {
                    position = position + (Vector3.down * .5f) + (direction * .5f)
                }
            };

            var previousParent = transform.parent;
            transform.parent = pivot.transform;

            var axis = Vector3.Cross(direction, Vector3.up).normalized;

            pivot.transform
                .DORotate(axis * -90, moveTime)
                .OnComplete(() => CleanupAfterAnimation(pivot, previousParent, finishCallback))
                .SetEase(Ease.Linear);
        }

        void CleanupAfterAnimation(Object pivot, Transform previousParent, Action<GridAnimation> finishCallback)
        {
            Movable.transform.parent = previousParent;
            Object.Destroy(pivot);
            finishCallback(this);
        }
    }
}