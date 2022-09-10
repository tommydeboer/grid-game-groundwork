namespace GridGame.Blocks.Rules
{
    public class BlockPushBlock : IGridInteraction<Block, Block>
    {
        public MoveResult Handle(Block pusher, Block pushee, Direction direction)
        {
            if (pushee.IsDynamic)
            {
                var pusheeMoveResult = pushee.Movable.TryMove(direction);
                return MoveResult.Of(pusheeMoveResult);
            }

            return MoveResult.Failed();
        }
    }
}