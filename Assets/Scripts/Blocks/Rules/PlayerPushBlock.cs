using GridGame.Player;

namespace GridGame.Blocks.Rules
{
    public class PlayerPushBlock : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic)
            {
                if (block.HasFaceAt(direction.Opposite()))
                {
                    var blockMoveResult = block.Movable.TryMove(direction);
                    return MoveResult.Of(blockMoveResult);
                }
                else
                {
                    return MoveResult.Success(direction.AsVector());
                }
            }
            else if (!block.HasFaceAt(direction.Opposite()))
            {
                return MoveResult.Success(direction.AsVector());
            }

            return MoveResult.Failed();
        }
    }
}