using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.SO;
using UnityEngine;

namespace GridGame
{
    public class Game : MonoBehaviour
    {
        readonly List<Triggerable> triggers = new();

        public void RegisterTrigger(Triggerable triggerable)
        {
            triggers.Add(triggerable);
        }

        public void UnregisterTrigger(Triggerable triggerable)
        {
            triggers.Remove(triggerable);
        }

        void CheckTriggers()
        {
            triggers.ForEach(trigger => trigger.Check());
        }
    }
}