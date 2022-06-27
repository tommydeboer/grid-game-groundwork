using UnityEngine;

namespace GridGame.Blocks
{
    public class EntityBehaviour : GridBehaviour
    {
        protected Entity Entity { get; private set; }

        protected virtual void Awake()
        {
            Entity = GetComponent<Entity>();
        }
    }
}