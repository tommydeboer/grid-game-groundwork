using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.SO;
using UnityEngine;

namespace GridGame
{
    public class Game : MonoBehaviour
    {
        [SerializeField]
        TurnLifecycleEventChannelSO turnLifecycleEventChannel;

        [SerializeField]
        UndoEventChannelSO undoEventChannel;

        public static Game instance;

        public static readonly List<Movable> moversToMove = new();

        public float moveTime = 0.18f; // time it takes to move 1 unit
        public float fallTime = 0.1f; // time it takes to fall 1 unit

        public static bool isMoving;
        public int movingCount;

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

        void OnEnable()
        {
            turnLifecycleEventChannel.OnMoveStart += DoScheduledMoves;
            turnLifecycleEventChannel.OnFallStart += FallStart;
        }

        void OnDisable()
        {
            turnLifecycleEventChannel.OnMoveStart -= DoScheduledMoves;
            turnLifecycleEventChannel.OnFallStart -= FallStart;
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

        void DoScheduledMoves()
        {
            if (moversToMove.Count == 0)
            {
                turnLifecycleEventChannel.EndMove();
            }

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
                turnLifecycleEventChannel.EndMove();
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
                turnLifecycleEventChannel.EndFall();
            }
        }

        void CheckTriggers()
        {
            triggers.ForEach(trigger => trigger.Check());
        }
    }
}