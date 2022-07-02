namespace GridGame.Blocks.Interactions
{
    public class BlockPushBlock : IGridInteraction<Block, Block>
    {
        public bool Handle(Block pusher, Block pushee, Direction direction)
        {
            if (pushee.IsDynamic)
            {
                return pushee.Movable.TryMove(direction.AsVector());
            }

            return false;
        }
    }
}