namespace GridGame.Blocks.Rules
{
    public class BlockPushBlock : IGridInteraction<Block, Block>
    {
        public MoveResult Handle(Block pusher, Block pushee, Direction direction)
        {
            if (pushee.IsDynamic)
            {
                bool pusheeDidMove = pushee.Movable.TryMove(direction.AsVector());
                return MoveResult.Of(pusheeDidMove, direction.AsVector());
            }

            return MoveResult.Failed();
        }
    }
}