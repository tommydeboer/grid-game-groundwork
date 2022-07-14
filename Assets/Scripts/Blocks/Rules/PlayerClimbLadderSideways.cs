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
                player.OnClimbable = blockNextToPlayer;
                player.LookAt(direction.AsVector());
                return MoveResult.Success((Hero.ClimbableOffset * ladder.Orientation.AsVector()) +
                                          (Hero.ClimbableOffset * direction.AsVector()));
            }

            if (blockNextToLadder && blockNextToLadder.IsOriented<Climbable>(ladder.Orientation))
            {
                if (blockNextToPlayer && blockNextToPlayer.IsDynamic)
                {
                    if (blockNextToPlayer.HasFaceAt(direction.Opposite()))
                    {
                        bool didMove = blockNextToPlayer.Movable.TryMove(direction);
                        if (didMove) player.OnClimbable = blockNextToLadder;
                        return MoveResult.Of(didMove, direction.AsVector());
                    }
                    else
                    {
                        return StepOff(player, ladder, direction);
                    }
                }

                player.OnClimbable = blockNextToLadder;
                return MoveResult.Success(direction.AsVector());
            }

            if (blockNextToPlayer && blockNextToPlayer.IsDynamic && blockNextToPlayer.HasFaceAt(direction.Opposite()))
            {
                bool didMove = blockNextToPlayer.Movable.TryMove(direction);
                if (didMove)
                {
                    StepOff(player, ladder, direction);
                }
                else
                {
                    return MoveResult.Failed();
                }
            }

            return StepOff(player, ladder, direction);
        }

        static MoveResult StepOff(Hero player, GridElement ladder, Direction direction)
        {
            player.Dismount();
            return MoveResult.Success(Hero.ClimbableOffset * ladder.Orientation.AsVector() + direction.AsVector());
        }
    }
}