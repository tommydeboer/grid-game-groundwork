using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Undo Event Channel")]
    public class UndoEventChannelSO : ScriptableObject
    {
        public UnityAction<bool> OnUndoRequested;
        public UnityAction OnResetRequested;
        public UnityAction OnSaveRequested;

        public void RequestUndo(bool cancelsCurrentMove)
        {
            OnUndoRequested?.Invoke(cancelsCurrentMove);
        }

        public void RequestReset()
        {
            OnResetRequested?.Invoke();
        }

        public void RequestSave()
        {
            OnSaveRequested?.Invoke();
        }
    }
}