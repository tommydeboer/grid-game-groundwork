using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public class PlayerClimbDownLadder : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block ladder, Direction direction)
        {
            Block blockBelowPlayer = player.BlockBelow;
            Block blockBelowLadder = ladder.BlockBelow;
            bool ladderContinuesBelow =
                blockBelowLadder && blockBelowLadder.IsOriented<Climbable>(direction);

            if (blockBelowPlayer || !ladderContinuesBelow)
            {
                player.Dismount();
                return MoveResult.Success(Hero.ClimbableOffset * (Vector3)direction.AsVector());
            }

            player.OnClimbable = blockBelowLadder;
            return MoveResult.Success(Direction.Down.AsVector());
        }
    }
}