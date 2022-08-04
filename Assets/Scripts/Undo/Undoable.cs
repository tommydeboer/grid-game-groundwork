using System;
using GridGame.SO;
using UnityEngine;

namespace GridGame.Undo
{
    public class Undoable : MonoBehaviour
    {
        [SerializeField]
        UndoEventChannelSO undoEventChannel;

        [SerializeField]
        TurnLifecycleEventChannelSO turnLifecycleEventChannel;

        IUndoable[] undoables;
        History<PersistableState>[] histories;

        void Awake()
        {
            undoables = GetComponents<IUndoable>();
            Init();
        }

        void OnEnable()
        {
            undoEventChannel.OnUndoRequested += Undo;
            undoEventChannel.OnResetRequested += Restart;
            turnLifecycleEventChannel.OnFallEnd += Save;
        }

        void OnDisable()
        {
            undoEventChannel.OnUndoRequested -= Undo;
            undoEventChannel.OnResetRequested -= Restart;
            turnLifecycleEventChannel.OnFallEnd -= Save;
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
            for (int i = 0; i < undoables.Length; i++)
            {
                PersistableState value = cancelsCurrentMove ? histories[i].Current() : histories[i].Back();
                undoables[i].ApplyState(value);
            }
        }

        void Restart()
        {
            for (int i = 0; i < undoables.Length; i++)
            {
                undoables[i].ApplyState(histories[i].Reset());
            }
        }

        void Save()
        {
            for (int i = 0; i < undoables.Length; i++)
            {
                histories[i].Push(undoables[i].GetState());
            }
        }
    }
}