using System;
using GridGame.Player;

namespace GridGame.Blocks.Rules
{
    public static class MoveHandler
    {
        static readonly IGridInteraction<Block, Block> blockPushBlock = new BlockPushBlock();
        static readonly IGridInteraction<Hero, Block> playerPushInsideBlock = new PlayerPushInsideBlock();
        static readonly IGridInteraction<Hero, Block> playerPushBlock = new PlayerPushBlock();
        static readonly IGridInteraction<Hero, Block> playerMountLadderTop = new PlayerMountLadderTop();
        static readonly IGridInteraction<Hero, Block> playerMountLadderBottom = new PlayerMountLadderBottom();
        static readonly IGridInteraction<Hero, Block> playerClimbDownLadder = new PlayerClimbDownLadder();
        static readonly IGridInteraction<Hero, Block> playerClimbUpLadder = new PlayerClimbUpLadder();

        public static MoveResult TryMove(GridElement element, Direction direction)
        {
            return element switch
            {
                Block block => TryMoveBlock(direction, block),
                Hero player => TryMovePlayer(direction, player),
                _ => throw new ArgumentException("No interaction implemented for " + element.name)
            };
        }

        static void TryMoveStacked(GridElement element, Direction direction)
        {
            if (element is Block block)
            {
                Block above = block.BlockAbove;
                if (above && above.IsDynamic)
                {
                    above.Movable.TryMove(direction.AsVector());
                }
            }
        }

        static MoveResult TryMoveBlock(Direction direction, Block block)
        {
            MoveResult result;
            Block target = block.GetNeighbour(direction.AsVector());

            if (!target)
            {
                result = MoveResult.Success(direction.AsVector());
            }
            else
            {
                result = blockPushBlock.Handle(block, target, direction);
            }

            if (result.DidMove)
            {
                TryMoveStacked(block, direction);
            }

            return result;
        }

        static MoveResult TryMovePlayer(Direction direction, Hero player)
        {
            if (player.OnClimbable) return TryPlayerClimb(direction, player);

            Block insideBlock = player.GetIntersects<Block>();
            if (insideBlock) return playerPushInsideBlock.Handle(player, insideBlock, direction);

            Block targetBlock = player.GetNeighbour(direction.AsVector());
            if (!targetBlock)
            {
                Block blockBelowPlayer = player.BlockBelow;
                if (blockBelowPlayer &&
                    blockBelowPlayer.IsOriented<Climbable>(direction.AsVector()) &&
                    !blockBelowPlayer.GetNeighbour(direction.AsVector()))
                {
                    return playerMountLadderTop.Handle(player, blockBelowPlayer, direction);
                }

                return MoveResult.Success(direction.AsVector());
            }

            if (targetBlock.IsOriented<Climbable>(direction.Opposite().AsVector()))
            {
                return playerMountLadderBottom.Handle(player, targetBlock, direction);
            }

            return playerPushBlock.Handle(player, targetBlock, direction);
        }

        static MoveResult TryPlayerClimb(Direction direction, Hero player)
        {
            if (player.GetNeighbour(direction.Opposite().AsVector()) == player.OnClimbable)
            {
                return playerClimbDownLadder.Handle(player, player.OnClimbable, direction);
            }

            if (player.GetNeighbour(direction.AsVector()) == player.OnClimbable)
            {
                return playerClimbUpLadder.Handle(player, player.OnClimbable, direction);
            }

            return MoveResult.Failed();
        }
    }
}