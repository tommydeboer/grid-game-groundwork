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

        public static readonly List<Movable> moversToMove = new();

        public float moveTime = 0.18f; // time it takes to move 1 unit
        public float fallTime = 0.1f; // time it takes to fall 1 unit

        public static bool isMoving;
        public int movingCount;
        public bool holdingUndo;
        public static bool isPolyban = true;

        readonly List<Movable> movables = new();
        readonly List<Triggerable> triggers = new();

        void Awake()
        {
            instance = this;
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

        public void RegisterMovable(Movable movable)
        {
            movables.Add(movable);
        }

        public void UnregisterMovable(Movable movable)
        {
            movables.Remove(movable);
        }

        public void RegisterTrigger(Triggerable triggerable)
        {
            triggers.Add(triggerable);
        }

        public void UnregisterTrigger(Triggerable triggerable)
        {
            triggers.Remove(triggerable);
        }

        /////////////////////////////////////////////////////////////////// MOVE

        public void DoScheduledMoves()
        {
            if (moversToMove.Count == 0) return;
            isMoving = true;
            foreach (Movable m in moversToMove)
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
                FallStart();
            }
        }

        void FallStart()
        {
            isMoving = true;
            movables.OrderBy(mover => mover.transform.position.y).ToList().ForEach(mover => mover.FallStart());

            if (movingCount == 0)
            {
                FallEnd();
            }
        }

        public void FallEnd()
        {
            if (movingCount == 0)
            {
                Refresh();
                CheckTriggers();
                CompleteMove();
            }
        }

        void CheckTriggers()
        {
            triggers.ForEach(trigger => trigger.Check());
        }


        public static void CompleteMove()
        {
            onMoveComplete?.Invoke();
        }
    }
}