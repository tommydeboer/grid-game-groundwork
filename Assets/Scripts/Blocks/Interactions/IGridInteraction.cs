namespace GridGame.Blocks.Interactions
{
    public interface IGridInteraction<in T1, in T2> where T1 : GridElement where T2 : GridElement
    {
        bool Handle(T1 element1, T2 element2, Direction direction);
    }
}