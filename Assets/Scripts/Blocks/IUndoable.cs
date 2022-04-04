namespace GridGame.Blocks
{
    public interface IUndoable
    {
        public object GetState();

        // TODO use other base class than object
        public void ApplyState(object values);
    }
}