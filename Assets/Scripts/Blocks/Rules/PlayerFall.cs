using GridGame.Player;

namespace GridGame.Blocks.Rules
{
    public class PlayerFall : IFall<Hero>
    {
        public bool ShouldFall(Hero player)
        {
            if (player.OnClimbable) return false;
            return !player.IsGrounded();
        }
    }
}