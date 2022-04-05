using System;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.Undo;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Player
{
    [RequireComponent(typeof(Movable))]
    public class Hero : BlockBehaviour, IUndoable
    {
        [SerializeField]
        FMODUnity.EventReference WalkEvent;

        [SerializeField]
        bool debugClimbables;

        public Block OnClimbable { get; set; }
        const float ClimbableOffset = 0.35f;

        Vector3Int currentMovementDir;
        Vector3Int mountDirection;
        public bool IsAlive { get; set; }

        Movable movable;
        Crushable crushable;

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

            if (currentMovementDir == Vector3Int.zero)
            {
                mountDirection = Vector3Int.zero;
            }
        }

        void LookAt(Vector3 dir)
        {
            var q = Quaternion.LookRotation(dir);
            transform.DORotate(q.eulerAngles, 0.1f);
        }

        void TryPlayerMove(Vector3Int dir)
        {
            Block target = Block.GetNeighbour(dir);
            bool targetIsEmpty = target == null;
            Block below = Block.GetNeighbour(Vector3Int.down);

            if (OnClimbable)
            {
                if ((dir != Vector3Int.forward && dir != Vector3Int.back) && mountDirection == dir)
                {
                    // prevent climbing when we just mounted a ladder sideways to prevent immediately stepping off
                    return;
                }

                TryClimb(dir, below);
                return;
            }

            Debug.Assert(below != null, "Trying to move player while not grounded");

            if (below.IsOriented<Climbable>(dir) && targetIsEmpty && below.HasEmptyAt(dir))
            {
                LogClimbableDebug("Mounting climbable from above");

                mountDirection = dir;
                Move(Vector3Int.down + (Vector3)dir * (1 - ClimbableOffset));
                OnClimbable = below;
                LookAt(-dir);
            }
            else if (!targetIsEmpty && target.IsOriented<Climbable>(-dir) && !Block.Intersects<BlockBehaviour>())
            {
                LogClimbableDebug("Mounting climbable");

                mountDirection = dir;
                Move((Vector3)dir * ClimbableOffset);
                OnClimbable = target;
                LookAt(dir);
            }
            else if (targetIsEmpty || target.Is<Empty>() || target.Is<Container>())
            {
                Move(dir);
                OnClimbable = null;
            }
            else if (target.Is<Movable>())
            {
                if (movable.TryMove(dir))
                {
                    Move(dir);
                    OnClimbable = null;
                }
            }


            if (!OnClimbable)
            {
                LookAt(dir);
            }
        }

        void TryClimb(Vector3Int dir, Block below)
        {
            // correct input direction based on climbable's orientation
            dir = Vector3Int.RoundToInt(Quaternion.Euler(OnClimbable.Tile.rot) * dir);

            var above = Block.GetNeighbour(Vector3Int.up);
            var opposite = Block.GetNeighbour(-dir);
            var target = Block.GetNeighbour(dir);
            var climbablePos = OnClimbable.Tile.gridPos;

            if (target != null && target == OnClimbable)
            {
                //TODO decide what to do: new behaviour "Solid"?
                if (above != null) return;

                var aboveClimbable = OnClimbable.GetNeighbour(Vector3Int.up);
                if (aboveClimbable != null && aboveClimbable.IsOriented<Climbable>(-dir))
                {
                    LogClimbableDebug("Climbing up climbable");

                    Move(Vector3Int.up);
                    OnClimbable = aboveClimbable;
                    LookAt(dir);
                }
                else if (aboveClimbable == null)
                {
                    LogClimbableDebug("Climbing up climbable over edge");

                    Move(Vector3Int.up + ((Vector3)dir * (1 - ClimbableOffset)));
                    OnClimbable = null;
                }

                return;
            }
            else if (OnClimbable.HasNeighbouringOriented<Climbable>(dir, OnClimbable.Orientation))
            {
                if (movable.TryMove(dir))
                {
                    LogClimbableDebug("Climbing to neighbouring climbable");

                    Move(dir);
                    // ReSharper disable once PossibleNullReferenceException
                    OnClimbable = OnClimbable.GetNeighbour(dir);
                }
            }
            else if (OnClimbable == opposite)
            {
                var belowClimbable = OnClimbable.GetNeighbour(Vector3Int.down);

                if (belowClimbable != null && belowClimbable.IsOriented<Climbable>(OnClimbable.Orientation) &&
                    below == null)
                {
                    LogClimbableDebug("Climbing down climbable");

                    Move(Vector3Int.down);
                    OnClimbable = belowClimbable;
                }
                else if (belowClimbable == null && below == null)
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
            else if (target != null && target.IsOriented<Climbable>(-dir))
            {
                LogClimbableDebug("Climbing to other climbable in corner");

                Vector3 directionToClimbable = ((Vector3)Block.Tile.gridPos - climbablePos).normalized;
                Move((directionToClimbable * ClimbableOffset) + ((Vector3)dir * ClimbableOffset));
                OnClimbable = target;
                LookAt(dir);
            }
            else
            {
                Vector3 directionToClimbable = ((Vector3)Block.Tile.gridPos - climbablePos).normalized;
                LogClimbableDebug("Stepping off climbable sideways");
                if (target != null && target.Is<Container>())
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
                Gizmos.DrawWireCube(OnClimbable.Tile.pos, Vector3.one);
            }

            Gizmos.color = Color.yellow;
            Transform tf = transform;
            Vector3 forward = tf.TransformDirection(Vector3.forward);
            Vector3 pos = tf.position;
            Gizmos.DrawLine(pos, pos + forward);
        }

        void LogClimbableDebug(string log)
        {
            if (debugClimbables)
            {
                Debug.Log(log);
            }
        }

        class State
        {
            public Block onClimbable;
            public bool isAlive;
        }

        public object GetState()
        {
            return new State
            {
                onClimbable = OnClimbable,
                isAlive = IsAlive
            };
        }

        public void ApplyState(object values)
        {
            var state = values as State;
            Debug.Assert(state != null, "Hero received a null undo state");

            IsAlive = state.isAlive;
            OnClimbable = state.onClimbable;
        }
    }
}