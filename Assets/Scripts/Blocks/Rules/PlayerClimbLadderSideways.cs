using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public class PlayerClimbLadderSideways : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block ladder, Direction direction)
        {
            Block blockNextToLadder = ladder.GetNeighbour(direction);
            Block blockNextToPlayer = player.GetNeighbour(direction);

            if (blockNextToPlayer && blockNextToPlayer.IsOriented<Climbable>(direction.Opposite()))
            {
                // climb to other ladder in corner
                player.OnClimbable = blockNextToPlayer;
                player.LookAt(direction.AsVector());
                return MoveResult.Success(Vector3.zero, MoveType.NONE);
            }

            if (blockNextToLadder && blockNextToLadder.IsOriented<Climbable>(ladder.Orientation))
            {
                if (blockNextToPlayer && blockNextToPlayer.IsDynamic)
                {
                    if (blockNextToPlayer.HasFaceAt(direction.Opposite()))
                    {
                        var blockMoveResult = blockNextToPlayer.Movable.TryMove(direction);
                        if (blockMoveResult.DidMove) player.OnClimbable = blockNextToLadder;
                        return MoveResult.Of(blockMoveResult);
                    }
                    else
                    {
                        return StepOff(player, direction);
                    }
                }

                player.OnClimbable = blockNextToLadder;
                return MoveResult.Success(direction.AsVector());
            }

            if (blockNextToPlayer && blockNextToPlayer.IsDynamic && blockNextToPlayer.HasFaceAt(direction.Opposite()))
            {
                var blockMoveResult = blockNextToPlayer.Movable.TryMove(direction);
                if (blockMoveResult.DidMove)
                {
                    StepOff(player, direction);
                }
                else
                {
                    return MoveResult.FailedBy(blockMoveResult);
                }
            }
            else if (blockNextToPlayer && !blockNextToPlayer.IsDynamic &&
                     blockNextToPlayer.HasFaceAt(direction.Opposite()))
            {
                return MoveResult.Failed();
            }

            return StepOff(player, direction);
        }

        static MoveResult StepOff(Hero player, Direction direction)
        {
            player.Dismount();
            return MoveResult.Success(direction.AsVector());
        }
    }
}