using System;
using GridGame.Blocks;
using GridGame.Blocks.Rules;
using UnityEngine;

namespace GridGame.Grid
{
    public static class GridAnimationFactory
    {
        public static GridAnimation Create(Movable movable, MoveResult moveResult)
        {
            var position = movable.transform.position;
            return moveResult.Type switch
            {
                MoveType.NONE => new NullAnimation
                {
                    Movable = movable
                },
                MoveType.SLIDE => new LinearAnimation
                {
                    Movable = movable, TargetPosition = position + moveResult.Vector
                },
                MoveType.TOPPLE => new ToppleAnimation
                {
                    Movable = movable, TargetPosition = position + moveResult.Vector
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}