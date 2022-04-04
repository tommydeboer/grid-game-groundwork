using System.Collections.Generic;

namespace GridGame.Blocks
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
                return history.Count == 0 ? initialValue : history.Peek();
            }
        }

        public T Current()
        {
            return history.Peek();
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