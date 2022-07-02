using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public class PlayerPushBlock : IGridInteraction<Hero, Block>
    {
        public bool Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic)
            {
                if (block.IsSolid || block.HasFaceAt(direction.Opposite()))
                {
                    return block.Movable.TryMove(direction.AsVector());
                }
                else
                {
                    return true;
                }
            }
            else if (!block.IsSolid && !block.HasFaceAt(direction.Opposite()))
            {
                return true;
            }

            return false;
        }
    }
}