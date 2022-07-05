using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Interactions
{
    public class PlayerClimbUpLadder : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block ladder, Direction direction)
        {
            Block blockAbovePlayer = player.BlockAbove;
            Block blockAboveLadder = ladder.BlockAbove;

            if (!blockAbovePlayer && !blockAboveLadder)
            {
                player.Dismount();
                return MoveResult.Success((1 - Hero.ClimbableOffset) * (Vector3)direction.AsVector() + Vector3.up);
            }

            bool ladderContinuesAbove =
                blockAboveLadder && blockAboveLadder.IsOriented<Climbable>(direction.Opposite().AsVector());
            if (ladderContinuesAbove)
            {
                player.OnClimbable = blockAboveLadder;
                return MoveResult.Success(Vector3.up);
            }

            return MoveResult.Failed();
        }
    }
}