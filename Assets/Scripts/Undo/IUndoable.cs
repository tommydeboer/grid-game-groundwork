namespace GridGame.Undo
{
    public interface IUndoable
    {
        public PersistableState GetState();

        public void ApplyState(PersistableState persistableState);
    }
}