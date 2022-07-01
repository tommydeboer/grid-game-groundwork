using UnityEngine;

namespace GridGame.Blocks
{
    public class BlockBehaviour : GridBehaviour
    {
        public Block Block { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            Block = GridElement as Block;
        }
    }
}