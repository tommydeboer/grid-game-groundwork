using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
            Game.onMoveComplete += cache.OnMoveComplete;
        }

        void OnDisable()
        {
            grid.OnGridReset -= cache.Reset;
            Game.onMoveComplete -= cache.OnMoveComplete;
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
            if (cache.undoIndex > 0)
            {
                DOTween.KillAll();
                if (Game.isMoving)
                {
                    Game.CompleteMove();
                }

                cache.DoUndo();
                game.Refresh();
            }
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
            cache.DoReset();
            game.Refresh();
        }
    }
}