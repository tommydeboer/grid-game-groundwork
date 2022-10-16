using System;
using UnityEngine;
using UnityEngine.Events;

namespace GridGame.SO
{
    [CreateAssetMenu(menuName = "SO/Game Loop Event Channel")]
    public class GameLoopEventChannelSO : ScriptableObject
    {
        /// Called when previous turn has ended. Nothing is moving, input can be processed. 
        public UnityAction OnInputStart;

        /// Called when input was received that leads to a move. 
        public UnityAction OnInputEnd;

        /// Called right before move animations are played. Moves have already been scheduled during input phase.
        public UnityAction OnMoveStart;

        /// Called when all move animations are finished.
        public UnityAction OnMoveEnd;

        /// Called after the move phase. See GridAnimator.ExecuteGravity()
        public UnityAction OnFallStart;

        /// Called when all falling objects have landed.
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

        public void Reset()
        {
            OnInputStart?.Invoke();
        }
    }
}