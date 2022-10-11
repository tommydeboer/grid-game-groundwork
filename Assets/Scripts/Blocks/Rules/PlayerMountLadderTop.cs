using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public class PlayerMountLadderTop : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block ladder, Direction direction)
        {
            player.Mount(ladder, direction.Opposite());
            return MoveResult.Success(direction.AsVector() + Vector3.down, MoveType.PLAYER_CLIMB_FROM_TOP);
        }
    }
}