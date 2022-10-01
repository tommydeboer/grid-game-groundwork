using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Rules
{
    public class PlayerMountLadderBottom : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block ladder, Direction direction)
        {
            player.Mount(ladder, direction);
            return MoveResult.Success(Vector3.zero, MoveType.NONE);
        }
    }
}