namespace GridGame.Blocks.Rules
{
    public class BlockFall : IFall<Block>
    {
        public bool ShouldFall(Block block)
        {
            return !IsGrounded(block);
        }

        /// Blocks are grounded if there is a non-moving block beneath them.
        /// Entities are ignored: blocks fall on top of them.
        static bool IsGrounded(GridElement block)
        {
            Block below = block.GetNeighbour(Direction.Down);
            if (!below) return false;

            Movable movableBelow = below.GetComponent<Movable>();
            if (movableBelow && movableBelow.isFalling)
            {
                return false;
            }

            return true;
        }
    }
}