using System;
using UnityEngine;
using UnityEngine.Events;

namespace GridGame.Blocks
{
    public class Triggerable : BlockBehaviour
    {
        [SerializeField]
        UnityEvent action;

        Game game;

        void Start()
        {
            game = CoreComponents.Game;
            game.RegisterTrigger(this);
        }

        public void Check()
        {
            action?.Invoke();
        }

        void OnDestroy()
        {
            game.UnregisterTrigger(this);
        }
    }
}