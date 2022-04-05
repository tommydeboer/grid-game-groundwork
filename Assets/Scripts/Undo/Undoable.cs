using GridGame.SO;
using UnityEngine;

namespace GridGame.Undo
{
    public class Undoable : MonoBehaviour
    {
        [SerializeField]
        UndoEventChannelSO undoEventChannel;

        IUndoable[] undoables;
        History<object>[] histories;

        void Awake()
        {
            undoables = GetComponents<IUndoable>();
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

        void Init()
        {
            histories = new History<object>[undoables.Length];
            for (int i = 0; i < undoables.Length; i++)
            {
                object initialValue = undoables[i].GetState();
                histories[i] = new History<object>(initialValue);
            }
        }

        void Undo(bool cancelsCurrentMove)
        {
            for (int i = 0; i < undoables.Length; i++)
            {
                object value = cancelsCurrentMove ? histories[i].Current() : histories[i].Back();
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