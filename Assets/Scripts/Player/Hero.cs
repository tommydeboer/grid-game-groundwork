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
        FMODUnity.EventReference WalkEvent;

        [SerializeField]
        bool debugClimbables;

        public Climbable OnClimbable { get; set; }
        const float ClimbableOffset = 0.35f;

        Vector3Int currentMovementDir;
        public bool IsAlive { get; set; }

        Movable movable;
        Crushable crushable;
        Grid grid;

        protected override void Awake()
        {
            base.Awake();
            movable = GetComponent<Movable>();
            crushable = GetComponent<Crushable>();
            IsAlive = true;
        }

        void OnEnable()
        {
            crushable.OnCrushed += OnCrush;
        }

        void OnDisable()
        {
            crushable.OnCrushed -= OnCrush;
        }

        void OnCrush()
        {
            IsAlive = false;
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

        bool CanInput()
        {
            return !Game.isMoving && !Game.instance.holdingUndo && IsAlive;
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
                Move(Vector3Int.down + (Vector3)dir * (1 - ClimbableOffset));
                OnClimbable = grid.Get<Climbable>(belowPlayer);
                LookAt(-dir);
            }
            else if (grid.HasOriented<Climbable>(targetPos, -dir) && !grid.Has<Container>(playerPos))
            {
                LogClimbableDebug("Mounting climbable");

                Move((Vector3)dir * ClimbableOffset);
                OnClimbable = grid.Get<Climbable>(targetPos);
                LookAt(dir);
            }
            else if (grid.Has<Container>(targetPos))
            {
                Move(dir);
                OnClimbable = null;
            }
            else if (grid.Has<Movable>(targetPos))
            {
                if (movable.TryMove(dir))
                {
                    Move(dir);
                    OnClimbable = null;
                }
            }
            else if (grid.IsEmpty(targetPos) || grid.Has<Empty>(targetPos))
            {
                Move(dir);
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

                    Move(Vector3Int.up);
                    OnClimbable = grid.Get<Climbable>(aboveClimbable);
                    LookAt(dir);
                }
                else if (grid.IsEmpty(aboveClimbable))
                {
                    LogClimbableDebug("Climbing up climbable over edge");

                    Move(Vector3Int.up + ((Vector3)dir * (1 - ClimbableOffset)));
                    OnClimbable = null;
                }
            }
            else if (grid.HasOriented<Climbable>(climbablePos + dir, OnClimbable.Block.Orientation))
            {
                if (movable.TryMove(dir))
                {
                    LogClimbableDebug("Climbing to neighbouring climbable");

                    Move(dir);
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

                    Move(Vector3Int.down);
                    OnClimbable = grid.Get<Climbable>(belowClimbable);
                }
                else if (grid.IsEmpty(belowClimbable) && grid.IsEmpty(belowPlayer))
                {
                    LogClimbableDebug("Falling down climbable");

                    Move(Vector3Int.down + ((Vector3)dir * ClimbableOffset));
                    OnClimbable = null;
                }
                else
                {
                    LogClimbableDebug("Stepping off climbable");

                    Move((Vector3)dir * ClimbableOffset);
                    OnClimbable = null;
                }
            }
            else if (grid.HasOriented<Climbable>(playerPos + dir, -dir))
            {
                LogClimbableDebug("Climbing to other climbable in corner");

                Vector3 directionToClimbable = ((Vector3)playerPos - climbablePos).normalized;
                Move((directionToClimbable * ClimbableOffset) + ((Vector3)dir * ClimbableOffset));
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
                    Move(dir + (directionToClimbable * ClimbableOffset));
                    OnClimbable = null;
                }
                else if (movable.TryMove(dir))
                {
                    Move(dir + (directionToClimbable * ClimbableOffset));
                    OnClimbable = null;
                }
            }

            if (!OnClimbable)
            {
                LookAt(dir);
            }
        }

        void Move(Vector3 dir)
        {
            FMODUnity.RuntimeManager.PlayOneShot(WalkEvent, transform.position);
            movable.ScheduleMove(dir);
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