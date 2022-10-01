using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
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
                return MoveResult.Success(direction.AsVector() + Vector3.up);
            }

            bool ladderContinuesAbove =
                blockAboveLadder && blockAboveLadder.IsOriented<Climbable>(direction.Opposite());
            if (ladderContinuesAbove)
            {
                player.OnClimbable = blockAboveLadder;
                return MoveResult.Success(Vector3.up);
            }

            return MoveResult.Failed();
        }
    }
}