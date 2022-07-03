using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public class PlayerPushBlock : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic)
            {
                if (block.IsSolid || block.HasFaceAt(direction.Opposite()))
                {
                    return MoveResult.Of(block.Movable.TryMove(direction.AsVector()));
                }
                else
                {
                    return MoveResult.Success();
                }
            }
            else if (!block.IsSolid && !block.HasFaceAt(direction.Opposite()))
            {
                return MoveResult.Success();
            }

            return MoveResult.Failed();
        }
    }
}