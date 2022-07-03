using System;
using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public static class MoveHandler
    {
        static readonly IGridInteraction<Block, Block> blockPushBlock = new BlockPushBlock();
        static readonly IGridInteraction<Hero, Block> playerPushInsideBlock = new PlayerPushInsideBlock();
        static readonly IGridInteraction<Hero, Block> playerPushBlock = new PlayerPushBlock();

        public static MoveResult TryMove(GridElement element, Direction direction)
        {
            return element switch
            {
                Block block => TryMoveBlock(direction, block),
                Hero player => TryMovePlayer(direction, player),
                _ => throw new ArgumentException("No interaction implemented for " + element.name)
            };
        }

        public static void TryMoveStacked(GridElement element, Direction direction)
        {
            if (element is Block block)
            {
                Block above = block.GetNeighbour(Direction.Up.AsVector());
                if (above && above.IsDynamic)
                {
                    above.Movable.TryMove(direction.AsVector());
                }
            }
        }

        static MoveResult TryMoveBlock(Direction direction, Block block)
        {
            Block target = block.GetNeighbour(direction.AsVector());
            if (!target) return MoveResult.Success();
            return blockPushBlock.Handle(block, target, direction);
        }

        static MoveResult TryMovePlayer(Direction direction, Hero player)
        {
            Block insideBlock = player.GetIntersects<Block>();
            if (insideBlock) return playerPushInsideBlock.Handle(player, insideBlock, direction);

            Block targetBlock = player.GetNeighbour(direction.AsVector());
            if (!targetBlock) return MoveResult.Success();
            return playerPushBlock.Handle(player, targetBlock, direction);
        }
    }
}