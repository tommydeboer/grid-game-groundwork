using GridGame.Player;
using UnityEngine;

namespace GridGame.Blocks.Interactions
{
    public class PlayerMountLadder : IGridInteraction<Hero, Block>
    {
        public MoveResult Handle(Hero player, Block block, Direction direction)
        {
            player.Mount(block, direction);
            return MoveResult.Success(Hero.ClimbableOffset * (Vector3)direction.AsVector());
        }
    }
}