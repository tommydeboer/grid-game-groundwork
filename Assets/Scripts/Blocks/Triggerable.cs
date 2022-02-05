using UnityEngine;
using UnityEngine.Events;

namespace GridGame.Blocks
{
    public class Triggerable : BlockBehaviour
    {
        [SerializeField]
        UnityEvent action;

        public void Check()
        {
            action?.Invoke();
        }
    }
}