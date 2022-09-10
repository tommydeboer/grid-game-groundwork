using GridGame.Player;

namespace GridGame.Blocks.Rules
{
    public class PlayerPushInsideBlock : IGridInteraction<Hero, Block>
    {
        static readonly IGridInteraction<Hero, Block> playerPushBlock = new PlayerPushBlock();

        public MoveResult Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic && block.HasFaceAt(direction))
            {
                if (block.HasFaceAt(Direction.Down))
                {
                    return MoveResult.Of(block.Movable.TryTopple(direction));
                }
                else
                {
                    return MoveResult.Of(block.Movable.TryMove(direction));
                }
            }
            else if (!block.HasFaceAt(direction))
            {
                Block target = player.GetNeighbour(direction);
                if (target)
                {
                    return playerPushBlock.Handle(player, target, direction);
                }

                return MoveResult.Success(direction.AsVector());
            }

            return MoveResult.Failed();
        }
    }
}