using System;
using GridGame.Player;

namespace GridGame.Blocks.Interactions
{
    public static class FallHandler
    {
        static readonly IFall<Hero> playerFall = new PlayerFall();
        static readonly IFall<Block> blockFall = new BlockFall();

        public static bool ShouldFall(GridElement element)
        {
            return element switch
            {
                Hero hero => playerFall.ShouldFall(hero),
                Block block => blockFall.ShouldFall(block),
                _ => throw new ArgumentException("No fall behaviour implemented for " + element.name)
            };
        }
    }
}