using GridGame.Player;

namespace GridGame.Blocks.Rules
{
    public class PlayerPushBlock : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic)
            {
                if (block.IsSolid || block.HasFaceAt(direction.Opposite()))
                {
                    bool blockDidMove = block.Movable.TryMove(direction);
                    return MoveResult.Of(blockDidMove, direction.AsVector());
                }
                else
                {
                    return MoveResult.Success(direction.AsVector());
                }
            }
            else if (!block.IsSolid && !block.HasFaceAt(direction.Opposite()))
            {
                return MoveResult.Success(direction.AsVector());
            }

            return MoveResult.Failed();
        }
    }
}