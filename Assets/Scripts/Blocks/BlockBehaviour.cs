using UnityEngine;

namespace GridGame.Blocks
{
    [RequireComponent(typeof(Block))]
    public class BlockBehaviour : MonoBehaviour
    {
        public Block Block { get; private set; }

        protected virtual void Awake()
        {
            Block = GetComponent<Block>();
        }
    }
}