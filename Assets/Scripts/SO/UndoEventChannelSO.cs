using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Undo Event Channel")]
    public class UndoEventChannelSO : ScriptableObject
    {
        [SerializeField]
        TurnLifecycleEventChannelSO turnLifecycleEventChannel;

        public UnityAction<bool> OnUndoRequested;
        public UnityAction OnResetRequested;

        public void RequestUndo(bool cancelsCurrentMove)
        {
            OnUndoRequested?.Invoke(cancelsCurrentMove);
            turnLifecycleEventChannel.CancelTurn();
        }

        public void RequestReset()
        {
            OnResetRequested?.Invoke();
            turnLifecycleEventChannel.CancelTurn();
        }
    }
}