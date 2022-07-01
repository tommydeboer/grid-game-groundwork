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

        /// A player is grounded if it is on top of a non-moving solid block. If the block
        /// is not solid a player is only grounded if the block contains a solid face at the
        /// top side.
        /// Entities are ignored: a player will fall on top of them.
        static bool IsGrounded(GridElement player)
        {
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