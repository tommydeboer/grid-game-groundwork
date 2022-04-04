using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GridGame.Blocks;
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

        Grid grid;
        Game game;
        UndoCache cache;

        bool holdingUndo;

        void Awake()
        {
            grid = CoreComponents.Grid;
            game = CoreComponents.Game;
            cache = new UndoCache(grid);
        }

        void OnEnable()
        {
            grid.OnGridReset += cache.Reset;
        }

        void OnDisable()
        {
            grid.OnGridReset -= cache.Reset;
        }

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
            DOTween.KillAll();
            undoEventChannel.RequestUndo(Game.isMoving);
            game.Refresh();
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
            DOTween.KillAll();
            game.Refresh();
            undoEventChannel.RequestReset();
        }
    }
}