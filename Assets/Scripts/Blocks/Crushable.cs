using UnityEngine.Events;

namespace GridGame.Blocks
{
    public class Crushable : BlockBehaviour
    {
        public UnityAction OnCrushed;

        public void Crush()
        {
            OnCrushed?.Invoke();
        }
    }
}