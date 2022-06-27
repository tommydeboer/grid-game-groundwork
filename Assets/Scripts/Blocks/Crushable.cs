using UnityEngine.Events;

namespace GridGame.Blocks
{
    public class Crushable : GridBehaviour
    {
        public UnityAction OnCrushed;

        public void Crush()
        {
            OnCrushed?.Invoke();
        }
    }
}