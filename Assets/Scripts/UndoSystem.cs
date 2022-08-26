using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.Grid;
using GridGame.SO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame
{
    public class UndoSystem : MonoBehaviour
    {
        [SerializeField]
        float holdUndoDelay = 0.75f;

        [SerializeField]
        float holdUndoInterval = 0.075f;

        [SerializeField]
        UndoEventChannelSO undoEventChannel;

        [SerializeField]
        GridAnimator gridAnimator;

        bool holdingUndo;

        [UsedImplicitly]
        public void OnUndo(InputValue value)
        {
            holdingUndo = value.isPressed;
            if (holdingUndo)
            {
                DoUndo();
                DOVirtual.DelayedCall(holdUndoDelay, UndoRepeat);
            }
            else
            {
                StartCoroutine(StopUndoing());
            }
        }

        [UsedImplicitly]
        public void OnReset(InputValue value)
        {
            DoReset();
        }

        void DoUndo()
        {
            GridAnimator.CancelAll();
            undoEventChannel.RequestUndo(gridAnimator.IsAnimating);
        }

        void UndoRepeat()
        {
            if (holdingUndo)
            {
                DoUndo();
                DOVirtual.DelayedCall(holdUndoInterval, UndoRepeat);
            }
        }

        IEnumerator StopUndoing()
        {
            yield return WaitFor.EndOfFrame;
            holdingUndo = false;
        }

        void DoReset()
        {
            GridAnimator.CancelAll();
            undoEventChannel.RequestReset();
        }
    }
}