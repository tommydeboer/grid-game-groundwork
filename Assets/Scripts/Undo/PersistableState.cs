using System;
using UnityEngine;

namespace GridGame.Undo
{
    public abstract class PersistableState
    {
        public T As<T>() where T : PersistableState
        {
            var state = this as T;
            Debug.Assert(state != null, "Received a null undo state");

            return state;
        }
    }
}