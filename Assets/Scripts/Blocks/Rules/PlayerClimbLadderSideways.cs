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

            if (blockNextToLadder && blockNextToLadder.IsOriented<Climbable>(ladder.Orientation))
            {
                if (blockNextToPlayer && blockNextToPlayer.IsDynamic)
                {
                    // TODO PlayerPushBlockOnLadder
                    return MoveResult.Failed();
                }

                player.OnClimbable = blockNextToLadder;
                return MoveResult.Success(direction.AsVector());
            }

            if (blockNextToLadder && blockNextToLadder.IsOriented<Climbable>(direction.Opposite()))
            {
                
            }

            player.Dismount();
            return MoveResult.Success(Hero.ClimbableOffset * ladder.Orientation.AsVector() + direction.AsVector());
        }
    }
}