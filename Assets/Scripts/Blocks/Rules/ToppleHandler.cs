using System;
using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public static class ToppleHandler
    {
        static readonly IGridInteraction<Block, Block> blockPushBlock = new BlockPushBlock();

        public static MoveResult TryTopple(GridElement element, Direction direction)
        {
            var result = element switch
            {
                Block block => TryToppleBlock(direction, block),
                _ => throw new ArgumentException("No interaction implemented for " + element.name)
            };

            return result;
        }

        static MoveResult TryToppleBlock(Direction direction, Block block)
        {
            Block above = block.BlockAbove;
            Block aboveTarget = Utils.GetBlockAtPos(block.Position + direction.AsVector() + Vector3.up);

            if (above || aboveTarget) return MoveResult.Failed();

            MoveResult result;
            Block target = block.GetNeighbour(direction);

            if (!target)
            {
                result = MoveResult.Success(direction.AsVector(), MoveType.TOPPLE);
            }
            else
            {
                result = blockPushBlock.Handle(block, target, direction);
                result.Type = MoveType.TOPPLE;
            }

            return result;
        }
    }
}