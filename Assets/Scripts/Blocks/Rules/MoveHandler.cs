using System;
using GridGame.Player;
using UnityEngine;

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
        static readonly IGridInteraction<Hero, Block> playerClimbLadderSideways = new PlayerClimbLadderSideways();

        public static MoveResult TryMove(GridElement element, Direction direction)
        {
            var result = element switch
            {
                Block block => TryMoveBlock(direction, block),
                Hero player => TryMovePlayer(direction, player),
                _ => throw new ArgumentException("No interaction implemented for " + element.name)
            };

            if (MovedOutOfBounds(element, result))
            {
                return MoveResult.Failed();
            }

            return result;
        }

        static void TryMoveStacked(GridElement element, Direction direction)
        {
            if (element is Block block)
            {
                Block above = block.BlockAbove;
                if (above && above.IsDynamic)
                {
                    above.Movable.TryMove(direction);
                }
            }
        }

        static MoveResult TryMoveBlock(Direction direction, Block block)
        {
            MoveResult result;
            Block target = block.GetNeighbour(direction);

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

            Block targetBlock = player.GetNeighbour(direction);
            if (!targetBlock)
            {
                Block blockBelowPlayer = player.BlockBelow;
                if (blockBelowPlayer &&
                    blockBelowPlayer.IsOriented<Climbable>(direction) &&
                    !blockBelowPlayer.GetNeighbour(direction))
                {
                    return playerMountLadderTop.Handle(player, blockBelowPlayer, direction);
                }

                return MoveResult.Success(direction.AsVector());
            }

            if (targetBlock.IsOriented<Climbable>(direction.Opposite()))
            {
                return playerMountLadderBottom.Handle(player, targetBlock, direction);
            }

            return playerPushBlock.Handle(player, targetBlock, direction);
        }

        static MoveResult TryPlayerClimb(Direction direction, Hero player)
        {
            if (player.GetNeighbour(direction.Opposite()) == player.OnClimbable)
            {
                return playerClimbDownLadder.Handle(player, player.OnClimbable, direction);
            }

            if (player.GetNeighbour(direction) == player.OnClimbable)
            {
                return playerClimbUpLadder.Handle(player, player.OnClimbable, direction);
            }

            return playerClimbLadderSideways.Handle(player, player.OnClimbable, direction);
        }

        static bool MovedOutOfBounds(GridElement element, MoveResult result)
        {
            if (result.DidMove)
            {
                return !(Physics.Raycast(
                    element.Position + result.Vector + Vector3.down * .45f,
                    Vector3.down,
                    out RaycastHit _,
                    Mathf.Infinity,
                    (int)Layers.GridPhysics));
            }

            return false;
        }
    }
}