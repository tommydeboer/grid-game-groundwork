namespace GridGame.Blocks.Rules
{
    public interface IFall<in T> where T : GridElement
    {
        bool ShouldFall(T t);
    }
}