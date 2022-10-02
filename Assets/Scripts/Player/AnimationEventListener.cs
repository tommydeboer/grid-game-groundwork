using System;
using JetBrains.Annotations;
using UnityEngine;

namespace GridGame.Player
{
    public class AnimationEventListener : MonoBehaviour
    {
        public Action AnimationCompletedCallback { private get; set; }

        [UsedImplicitly]
        public void OnAnimationCompleted()
        {
            AnimationCompletedCallback();
            AnimationCompletedCallback = null;
        }
    }
}