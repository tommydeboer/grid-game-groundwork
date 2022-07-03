namespace GridGame.Blocks.Interactions
{
    public class BlockPushBlock : IGridInteraction<Block, Block>
    {
        public MoveResult Handle(Block pusher, Block pushee, Direction direction)
        {
            if (pushee.IsDynamic)
            {
                return MoveResult.Of(pushee.Movable.TryMove(direction.AsVector()));
            }

            return MoveResult.Failed();
        }
    }
}