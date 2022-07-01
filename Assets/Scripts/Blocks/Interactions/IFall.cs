namespace GridGame.Blocks.Interactions
{
    public interface IFall<in T> where T : GridElement
    {
        bool ShouldFall(T t);
    }
}