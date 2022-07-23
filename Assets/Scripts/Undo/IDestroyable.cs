namespace GridGame.Undo
{
    public interface IRemovable
    {
        public void OnRemove();

        public void OnReplace();
    }
}