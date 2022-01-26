using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Blocks;
using UnityEngine;

namespace GridGame
{
    public class Game : MonoBehaviour
    {
        public delegate void GameEvent();

        public static GameEvent onUndo;
        public static GameEvent onReset;
        public static GameEvent onMoveComplete;

        public static Game instance;

        public static readonly List<Mover> moversToMove = new();

        public float moveTime = 0.18f; // time it takes to move 1 unit
        public float fallTime = 0.1f; // time it takes to fall 1 unit

        public static bool isMoving;
        public int movingCount;
        public bool holdingUndo;
        public static bool isPolyban = true;

        Grid grid;

        void Awake()
        {
            instance = this;
            Application.targetFrameRate = 60;
            grid = CoreComponents.Grid;
        }

        void Start()
        {
            // TODO uncomment when undo system is back online
            // State.Init();
            isMoving = false;
        }

        void Update()
        {
            // TODO uncomment when undo system is back online
            // if (Input.GetKeyDown(KeyCode.Z))
            // {
            //     holdingUndo = true;
            //     DoUndo();
            //     DOVirtual.DelayedCall(0.75f, UndoRepeat);
            // }
            // else if (Input.GetKeyDown(KeyCode.R))
            // {
            //     DoReset();
            // }
            //
            // if (Input.GetKeyUp(KeyCode.Z))
            // {
            //     StartCoroutine(StopUndoing());
            // }
        }

        void Refresh()
        {
            isMoving = false;
            Debug.Assert(movingCount == 0, "Not all movers have finished");
            moversToMove.Clear();
            movingCount = 0;
        }

        /////////////////////////////////////////////////////////////////// UNDO / RESET
        // TODO uncomment when undo system is back online
        //
        // void DoReset()
        // {
        //     DOTween.KillAll();
        //     isMoving = false;
        //     State.DoReset();
        //     Refresh();
        //     if (onReset != null)
        //     {
        //         onReset();
        //     }
        // }
        //
        // void DoUndo()
        // {
        //     if (State.undoIndex > 0)
        //     {
        //         DOTween.KillAll();
        //         if (isMoving)
        //         {
        //             CompleteMove();
        //         }
        //
        //         isMoving = false;
        //         State.DoUndo();
        //         Refresh();
        //         if (onUndo != null)
        //         {
        //             onUndo();
        //         }
        //     }
        // }
        //
        // void UndoRepeat()
        // {
        //     if (Input.GetKey(KeyCode.Z) && holdingUndo)
        //     {
        //         DoUndo();
        //         DOVirtual.DelayedCall(0.075f, UndoRepeat);
        //     }
        // }
        //
        // IEnumerator StopUndoing()
        // {
        //     yield return WaitFor.EndOfFrame;
        //     holdingUndo = false;
        // }

        /////////////////////////////////////////////////////////////////// MOVE

        public void DoScheduledMoves()
        {
            if (moversToMove.Count == 0) return;
            isMoving = true;
            foreach (Mover m in moversToMove)
            {
                movingCount++;
                m.transform.DOMove(m.goalPosition, moveTime).OnComplete(MoveEnd).SetEase(Ease.Linear);
            }
        }

        void MoveEnd()
        {
            movingCount--;
            if (movingCount == 0)
            {
                grid.Refresh();
                FallStart();
            }
        }

        void FallStart()
        {
            isMoving = true;
            grid.GetMovers().OrderBy(mover => mover.transform.position.y).ToList().ForEach(mover => mover.FallStart());

            if (movingCount == 0)
            {
                FallEnd();
            }
        }

        public void FallEnd()
        {
            grid.Refresh();
            if (movingCount == 0)
            {
                Refresh();
                CheckTriggers();
                CompleteMove();
            }
        }

        void CheckTriggers()
        {
            grid.GetTriggers().ForEach(trigger => trigger.Check());
        }


        static void CompleteMove()
        {
            // TODO uncomment when undo system is back online
            // State.OnMoveComplete();
            onMoveComplete?.Invoke();
        }
    }
}