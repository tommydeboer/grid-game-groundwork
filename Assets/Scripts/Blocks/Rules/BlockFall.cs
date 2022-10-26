namespace GridGame.Blocks.Rules
{
    public class BlockFall : IFall<Block>
    {
        public bool ShouldFall(Block block)
        {
            return !block.IsGrounded();
        }
    }
}