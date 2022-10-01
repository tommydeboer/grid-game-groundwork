using System;
using DG.Tweening;
using UnityEngine;

namespace GridGame.Grid
{
    internal record NullAnimation : GridAnimation
    {
        public override void Play(float moveTime, Action<GridAnimation> finishCallback)
        {
            DOTween.Sequence().PrependInterval(moveTime).OnComplete(() => finishCallback(this));
        }
    }
}