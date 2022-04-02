using System;
using UnityEngine;
using UnityEngine.Events;

namespace GridGame.Blocks
{
    public class Triggerable : BlockBehaviour
    {
        [SerializeField]
        UnityEvent action;

        void Start()
        {
            CoreComponents.Game.RegisterTrigger(this);
        }

        public void Check()
        {
            action?.Invoke();
        }

        void OnDestroy()
        {
            CoreComponents.Game.UnregisterTrigger(this);
        }
    }
}