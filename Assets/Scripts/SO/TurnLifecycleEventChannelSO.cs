using System;
using UnityEngine;
using UnityEngine.Events;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Turn Lifecycle Event Channel")]
    public class TurnLifecycleEventChannelSO : ScriptableObject
    {
        public UnityAction OnInputStart;
        public UnityAction OnInputEnd;
        public UnityAction OnMoveStart;
        public UnityAction OnMoveEnd;
        public UnityAction OnFallStart;
        public UnityAction OnFallEnd;


        public void EndInput()
        {
            OnInputEnd?.Invoke();
            OnMoveStart?.Invoke();
        }

        public void EndMove()
        {
            OnMoveEnd?.Invoke();
            OnFallStart?.Invoke();
        }

        public void EndFall()
        {
            OnFallEnd?.Invoke();
            OnInputStart?.Invoke();
        }

        public void CancelTurn()
        {
            OnInputStart?.Invoke();
        }
    }
}