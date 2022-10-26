namespace GridGame.Blocks
{
    public class Entity : GridElement
    {
        /// An entity is grounded if:
        /// - it is on top of a non-moving solid block.
        /// - it is on top of a non-moving non-solid block with a solid face on top
        /// - it is inside a non-moving block with a solid face at the bottom
        /// Entities are ignored: entities fall on top of each other
        public override bool IsGrounded()
        {
            Block insideBlock = GetIntersects<Block>();
            if (insideBlock && insideBlock.HasFaceAt(Direction.Down))
            {
                if (insideBlock.IsDynamic && insideBlock.Movable.IsFalling)
                {
                    return false;
                }

                return true;
            }

            Block below = GetNeighbour(Direction.Down);
            if (!below) return false;

            Movable movableBelow = below.Movable;
            if (movableBelow && movableBelow.IsFalling)
            {
                return false;
            }

            return below.IsSolid || below.HasFaceAt(Direction.Up);
        }
    }
}