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
            isMoving = false;
        }

        public void Refresh()
        {
            isMoving = false;
            moversToMove.Clear();
            movingCount = 0;
        }

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


        public static void CompleteMove()
        {
            onMoveComplete?.Invoke();
        }
    }
}