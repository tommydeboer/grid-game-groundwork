using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public class PlayerFall : IFall<Hero>
    {
        public bool ShouldFall(Hero player)
        {
            if (player.OnClimbable) return false;
            return !IsGrounded(player);
        }

        /// A player is grounded if:
        /// - it is on top of a non-moving solid block.
        /// - it is on top of a non-moving non-solid block with a solid face on top
        /// - it is inside a block with a solid face at the bottom
        /// Entities are ignored: a player will fall on top of them.
        static bool IsGrounded(GridElement player)
        {
            Block insideBlock = player.GetIntersects<Block>();
            if (insideBlock && insideBlock.HasFaceAt(Direction.Down))
            {
                return true;
            }

            Block below = player.GetNeighbour(Direction.Down.AsVector());
            if (!below) return false;

            Movable movableBelow = below.GetComponent<Movable>();
            if (movableBelow && movableBelow.isFalling)
            {
                return false;
            }

            return below.IsSolid || below.HasFaceAt(Direction.Up);
        }
    }
}