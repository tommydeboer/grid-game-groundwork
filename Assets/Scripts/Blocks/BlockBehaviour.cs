using UnityEngine;

namespace GridGame.Blocks
{
    public class BlockBehaviour : GridBehaviour
    {
        public Block Block { get; protected set; }

        protected virtual void Awake()
        {
            Block = GetComponent<Block>();
        }
    }
}