using System;
using GridGame.Blocks;
using GridGame.Blocks.Rules;
using GridGame.Player;
using UnityEngine;

namespace GridGame.Grid
{
    public static class GridAnimationFactory
    {
        public static GridAnimation Create(Movable movable, MoveResult moveResult,
            AnimationEventListener animationEventListener)
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
                MoveType.PLAYER_CLIMB_ON_TOP => InstantAnimation(movable, moveResult, position, animationEventListener),
                MoveType.PLAYER_CLIMB_FROM_TOP => InstantAnimation(movable, moveResult, position,
                    animationEventListener),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static InstantAnimation InstantAnimation(Movable movable, MoveResult moveResult, Vector3 position,
            AnimationEventListener animationEventListener)
        {
            var animation = new InstantAnimation
            {
                Movable = movable, TargetPosition = position + moveResult.Vector
            };
            animationEventListener.AnimationCompletedCallback = animation.AnimationCallback;
            return animation;
        }
    }
}