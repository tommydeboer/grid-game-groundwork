using UnityEngine;

namespace GridGame.Blocks
{
    public class BlockBehaviour : MonoBehaviour
    {
        public Block Block { get; protected set; }

        protected virtual void Awake()
        {
            Block = GetComponent<Block>();
        }
    }
}