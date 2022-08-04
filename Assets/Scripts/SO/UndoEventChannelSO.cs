using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Undo Event Channel")]
    public class UndoEventChannelSO : ScriptableObject
    {
        [SerializeField]
        GameLoopEventChannelSO gameLoopEventChannelSo;

        public UnityAction<bool> OnUndoRequested;
        public UnityAction OnResetRequested;

        public void RequestUndo(bool cancelsCurrentMove)
        {
            OnUndoRequested?.Invoke(cancelsCurrentMove);
            gameLoopEventChannelSo.Reset();
        }

        public void RequestReset()
        {
            OnResetRequested?.Invoke();
            gameLoopEventChannelSo.Reset();
        }
    }
}