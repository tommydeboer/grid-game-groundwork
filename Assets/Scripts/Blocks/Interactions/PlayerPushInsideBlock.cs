using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public class PlayerPushInsideBlock : IGridInteraction<Hero, Block>
    {
        static readonly IGridInteraction<Hero, Block> playerPushBlock = new PlayerPushBlock();

        public bool Handle(Hero player, Block block, Direction direction)
        {
            if (block.IsDynamic && block.HasFaceAt(direction))
            {
                if (block.HasFaceAt(Direction.Down)) return false;
                return block.Movable.TryMove(direction.AsVector());
            }
            else if (!block.HasFaceAt(direction))
            {
                Block target = player.GetNeighbour(direction.AsVector());
                if (target)
                {
                    return playerPushBlock.Handle(player, target, direction);
                }

                return true;
            }

            return false;
        }
    }
}