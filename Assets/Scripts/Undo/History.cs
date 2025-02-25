using System.Collections.Generic;

namespace GridGame.Undo
{
    public class History<T>
    {
        readonly T initialValue;
        readonly Stack<T> history = new();

        public History(T initialValue)
        {
            this.initialValue = initialValue;
        }

        public T Back()
        {
            if (history.Count == 0)
            {
                return initialValue;
            }
            else
            {
                history.Pop();
                return Current();
            }
        }

        public T Current()
        {
            return history.Count == 0 ? initialValue : history.Peek();
        }

        public void Push(T value)
        {
            history.Push(value);
        }

        public T Reset()
        {
            history.Clear();
            return initialValue;
        }
    }
}