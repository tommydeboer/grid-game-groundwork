using System;
using DG.Tweening;
using GridGame.Blocks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Player
{
    [RequireComponent(typeof(Movable))]
    public class Hero : BlockBehaviour
    {
        [SerializeField]
        bool debugClimbables;

        public Climbable OnClimbable { get; set; }
        const float ClimbableOffset = 0.35f;

        Vector3Int currentMovementDir;

        Movable movable;
        Grid grid;

        protected override void Awake()
        {
            base.Awake();
            movable = GetComponent<Movable>();
        }

        void Start()
        {
            grid = CoreComponents.Grid;

            if (Camera.main != null)
            {
                var playerCamera = Camera.main.GetComponent<PlayerCamera>();
                playerCamera.Player = this;
            }
            else
            {
                Debug.LogWarning("No main camera. Is the persistent scene loaded?");
            }
        }

        void Update()
        {
            if (CanInput() && currentMovementDir != Vector3Int.zero)
            {
                TryPlayerMove(currentMovementDir);
                Game.instance.DoScheduledMoves();
            }
        }

        static bool CanInput()
        {
            return !Game.isMoving && !Game.instance.holdingUndo;
        }

        [UsedImplicitly]
        public void OnMove(InputValue value)
        {
            var movement = value.Get<Vector2>();
            currentMovementDir = new Vector3Int((int)movement.x, 0, (int)movement.y);
        }

        void LookAt(Vector3 dir)
        {
            var q = Quaternion.LookRotation(dir);
            transform.DORotate(q.eulerAngles, 0.1f);
        }

        void TryPlayerMove(Vector3Int dir)
        {
            var playerPos = Block.Tile.gridPos;
            Vector3Int targetPos = playerPos + dir;
            var belowPlayer = playerPos + Vector3Int.down;

            if (OnClimbable)
            {
                TryClimb(dir, playerPos, belowPlayer);
                return;
            }
            else if (grid.HasOriented<Climbable>(belowPlayer, dir) && grid.IsEmpty(targetPos) &&
                     grid.IsEmpty(targetPos + Vector3Int.down))
            {
                LogClimbableDebug("Mounting climbable from above");

                // mount climbable from above
                movable.ScheduleMove(Vector3Int.down + (Vector3)dir * (1 - ClimbableOffset));
                OnClimbable = grid.Get<Climbable>(belowPlayer);
                LookAt(-dir);
            }
            else if (grid.HasOriented<Climbable>(targetPos, -dir) && !grid.Has<Container>(playerPos))
            {
                LogClimbableDebug("Mounting climbable");

                movable.ScheduleMove((Vector3)dir * ClimbableOffset);
                OnClimbable = grid.Get<Climbable>(targetPos);
                LookAt(dir);
            }
            else if (grid.Has<Container>(targetPos))
            {
                movable.ScheduleMove(dir);
                OnClimbable = null;
            }
            else if (grid.Has<Movable>(targetPos))
            {
                if (movable.TryMove(dir))
                {
                    movable.ScheduleMove(dir);
                    OnClimbable = null;
                }
            }
            else if (grid.IsEmpty(targetPos) || grid.Has<Empty>(targetPos))
            {
                movable.ScheduleMove(dir);
                OnClimbable = null;
            }

            if (!OnClimbable)
            {
                LookAt(dir);
            }
        }

        void TryClimb(Vector3Int dir, Vector3Int playerPos, Vector3Int belowPlayer)
        {
            // correct input direction based on climbable's orientation
            dir = Vector3Int.RoundToInt(Quaternion.Euler(OnClimbable.Block.Tile.rot) * dir);
            Vector3Int targetPos = playerPos + dir;

            var abovePlayer = playerPos + Vector3Int.up;
            var climbablePos = OnClimbable.Block.Tile.gridPos;
            if (targetPos == climbablePos)
            {
                //TODO decide what to do: new behaviour "Solid"?
                if (grid.Has<BlockBehaviour>(abovePlayer)) return;

                var aboveClimbable = targetPos + Vector3Int.up;
                if (grid.HasOriented<Climbable>(aboveClimbable, -dir))
                {
                    LogClimbableDebug("Climbing up climbable");

                    movable.ScheduleMove(Vector3Int.up);
                    OnClimbable = grid.Get<Climbable>(aboveClimbable);
                    LookAt(dir);
                }
                else if (grid.IsEmpty(aboveClimbable))
                {
                    LogClimbableDebug("Climbing up climbable over edge");

                    movable.ScheduleMove(Vector3Int.up + ((Vector3)dir * (1 - ClimbableOffset)));
                    OnClimbable = null;
                }
            }
            else if (grid.HasOriented<Climbable>(climbablePos + dir, OnClimbable.Block.Orientation))
            {
                if (movable.TryMove(dir))
                {
                    LogClimbableDebug("Climbing to neighbouring climbable");

                    movable.ScheduleMove(dir);
                    OnClimbable = grid.Get<Climbable>(climbablePos + dir);
                }
            }
            else if (OnClimbable == grid.Get<Climbable>(playerPos - dir))
            {
                var belowClimbable = climbablePos + Vector3Int.down;

                if (grid.HasOriented<Climbable>(belowClimbable, OnClimbable.Block.Orientation) &&
                    grid.IsEmpty(belowPlayer))
                {
                    LogClimbableDebug("Climbing down climbable");

                    movable.ScheduleMove(Vector3Int.down);
                    OnClimbable = grid.Get<Climbable>(belowClimbable);
                }
                else if (grid.IsEmpty(belowClimbable) && grid.IsEmpty(belowPlayer))
                {
                    LogClimbableDebug("Falling down climbable");

                    movable.ScheduleMove(Vector3Int.down + ((Vector3)dir * ClimbableOffset));
                    OnClimbable = null;
                }
                else
                {
                    LogClimbableDebug("Stepping off climbable");

                    movable.ScheduleMove((Vector3)dir * ClimbableOffset);
                    OnClimbable = null;
                }
            }
            else if (grid.HasOriented<Climbable>(playerPos + dir, -dir))
            {
                LogClimbableDebug("Climbing to other climbable in corner");

                Vector3 directionToClimbable = ((Vector3)playerPos - climbablePos).normalized;
                movable.ScheduleMove((directionToClimbable * ClimbableOffset) + ((Vector3)dir * ClimbableOffset));
                OnClimbable = grid.Get<Climbable>(playerPos + dir);
                LookAt(dir);
            }
            else
            {
                Vector3 directionToClimbable = ((Vector3)playerPos - climbablePos).normalized;
                LogClimbableDebug("Stepping off climbable sideways");
                var movableAtPos = grid.Get<Movable>(targetPos);
                if (movableAtPos != null && movableAtPos.gameObject.GetComponent<Container>())
                {
                    movable.ScheduleMove(dir + (directionToClimbable * ClimbableOffset));
                    OnClimbable = null;
                }
                else if (movable.TryMove(dir))
                {
                    movable.ScheduleMove(dir + (directionToClimbable * ClimbableOffset));
                    OnClimbable = null;
                }
            }

            if (!OnClimbable)
            {
                LookAt(dir);
            }
        }


        public void OnDrawGizmos()
        {
            if (debugClimbables && OnClimbable)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(OnClimbable.Block.Tile.pos, Vector3.one);
            }
        }

        void LogClimbableDebug(string log)
        {
            if (debugClimbables)
            {
                Debug.Log(log);
            }
        }
    }
}