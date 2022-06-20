using UnityEngine;

namespace GridGame.Blocks
{
    public class Face : BlockBehaviour
    {
        protected override void Awake()
        {
            // Block faces get Block from the parent
            Block = GetComponentInParent<Block>();
        }
    }
}