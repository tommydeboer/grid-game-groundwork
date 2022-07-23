using System;
using GridGame.SO;
using UnityEngine;

namespace GridGame.Undo
{
    public class Undoable : MonoBehaviour
    {
        [SerializeField]
        UndoEventChannelSO undoEventChannel;

        IUndoable[] undoables;
        IRemovable[] removables;
        History<PersistableState>[] histories;

        int index;
        int removedAtIndex = -1;

        void Awake()
        {
            undoables = GetComponents<IUndoable>();
            removables = GetComponents<IRemovable>();
            Init();
        }

        void OnEnable()
        {
            undoEventChannel.OnUndoRequested += Undo;
            undoEventChannel.OnResetRequested += Restart;
            undoEventChannel.OnSaveRequested += Save;
        }

        void OnDisable()
        {
            undoEventChannel.OnUndoRequested -= Undo;
            undoEventChannel.OnResetRequested -= Restart;
            undoEventChannel.OnSaveRequested -= Save;
        }

        public void Remove()
        {
            removedAtIndex = index;
            foreach (IRemovable removable in removables)
            {
                removable.OnRemove();
            }
        }

        void Replace()
        {
            removedAtIndex = -1;
            foreach (IRemovable removable in removables)
            {
                removable.OnReplace();
            }
        }

        void Init()
        {
            histories = new History<PersistableState>[undoables.Length];
            for (int i = 0; i < undoables.Length; i++)
            {
                PersistableState initialValue = undoables[i].GetState();
                histories[i] = new History<PersistableState>(initialValue);
            }
        }

        void Undo(bool cancelsCurrentMove)
        {
            index = Math.Max(0, index - 1);
            int checkIndex = cancelsCurrentMove ? removedAtIndex - 1 : removedAtIndex;
            if (index == checkIndex)
            {
                Replace();
            }

            for (int i = 0; i < undoables.Length; i++)
            {
                PersistableState value = cancelsCurrentMove ? histories[i].Current() : histories[i].Back();
                undoables[i].ApplyState(value);
            }
        }

        void Restart()
        {
            if (removedAtIndex > -1) Replace();
            index = 0;
            removedAtIndex = -1;

            for (int i = 0; i < undoables.Length; i++)
            {
                undoables[i].ApplyState(histories[i].Reset());
            }
        }

        void Save()
        {
            index++;
            if (removedAtIndex > -1) return;

            for (int i = 0; i < undoables.Length; i++)
            {
                histories[i].Push(undoables[i].GetState());
            }
        }
    }
}