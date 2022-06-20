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

        public Block OnClimbable { get; private set; }
        const float ClimbableOffset = 0.35f;

        Vector3Int currentMovementDir;
        Vector3Int mountDirection;
        bool IsAlive { get; set; }

        Movable movable;
        Crushable crushable;
        bool holdingUndo;

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
            return !Game.isMoving && !holdingUndo && IsAlive;
        }

        [UsedImplicitly]
        public void OnUndo(InputValue value)
        {
            holdingUndo = value.isPressed;
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

        void SetClimbable(Block block)
        {
            OnClimbable = block;
            Block.AttachedTo = block;
        }

        void ResetClimbable()
        {
            OnClimbable = null;
            Block.AttachedTo = null;
        }

        void TryPlayerMove(Vector3Int dir)
        {
            Block target = GetNearestBlock(dir);
            bool targetIsEmpty = target == null;

            // TODO change with more general ground check (solid block or solid face)
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
                SetClimbable(below);
                LookAt(-dir);
            }
            else if (!targetIsEmpty && target.IsOriented<Climbable>(-dir) && !Block.Intersects<BlockBehaviour>())
            {
                LogClimbableDebug("Mounting climbable");

                mountDirection = dir;
                Move((Vector3)dir * ClimbableOffset);
                SetClimbable(target);
                LookAt(dir);
            }
            else if (targetIsEmpty)
            {
                Move(dir);
                ResetClimbable();
            }
            else if (target.IsDynamic && !target.IsSolid)
            {
                // TODO refactor
                if (target.Position == Block.Position)
                {
                    // standing inside container moving outward
                    if (target.HasFaceAt(dir.ToDirection()))
                    {
                        if (movable.TryMove(dir, target))
                        {
                            Move(dir);
                            ResetClimbable();
                        }
                    }
                    else
                    {
                        Move(dir);
                        ResetClimbable();
                    }
                }
                else
                {
                    // standing next to container moving inward
                    if (target.HasFaceAt((-dir).ToDirection()))
                    {
                        if (movable.TryMove(dir))
                        {
                            Move(dir);
                            ResetClimbable();
                        }
                    }
                    else
                    {
                        Move(dir);
                        ResetClimbable();
                    }
                }
            }
            else if (target.IsDynamic)
            {
                if (movable.TryMove(dir))
                {
                    Move(dir);
                    ResetClimbable();
                }
            }
            else if (!target.IsSolid && !target.HasFaceAt((-dir).ToDirection()))
            {
                Move(dir);
                ResetClimbable();
            }


            if (!OnClimbable)
            {
                LookAt(dir);
            }
        }

        [CanBeNull]
        Block GetNearestBlock(Vector3Int direction)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, (int)Layers.GridPhysics))
            {
                if (hit.collider.gameObject.GetComponent<Face>())
                {
                    return hit.collider.gameObject.GetComponent<Face>().Block;
                }

                return hit.collider.gameObject.GetComponentInParent<Block>();
            }

            return null;
        }

        void TryClimb(Vector3Int dir, Block below)
        {
            // correct input direction based on climbable's orientation
            dir = Vector3Int.RoundToInt(Quaternion.Euler(OnClimbable.Rotation) * dir);

            var above = Block.GetNeighbour(Vector3Int.up);
            var opposite = Block.GetNeighbour(-dir);
            var target = Block.GetNeighbour(dir);
            var climbablePos = OnClimbable.Position;

            if (target != null && target == OnClimbable)
            {
                //TODO decide what to do: new behaviour "Solid"?
                if (above != null) return;

                var aboveClimbable = OnClimbable.GetNeighbour(Vector3Int.up);
                if (aboveClimbable != null && aboveClimbable.IsOriented<Climbable>(-dir))
                {
                    LogClimbableDebug("Climbing up climbable");

                    Move(Vector3Int.up);
                    SetClimbable(aboveClimbable);
                    LookAt(dir);
                }
                else if (aboveClimbable == null)
                {
                    LogClimbableDebug("Climbing up climbable over edge");

                    Move(Vector3Int.up + ((Vector3)dir * (1 - ClimbableOffset)));
                    ResetClimbable();
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
                    SetClimbable(OnClimbable.GetNeighbour(dir));
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
                    SetClimbable(belowClimbable);
                }
                else if (belowClimbable == null && below == null)
                {
                    LogClimbableDebug("Falling down climbable");

                    Move(Vector3Int.down + ((Vector3)dir * ClimbableOffset));
                    ResetClimbable();
                }
                else
                {
                    LogClimbableDebug("Stepping off climbable");

                    Move((Vector3)dir * ClimbableOffset);
                    ResetClimbable();
                }
            }
            else if (target != null && target.IsOriented<Climbable>(-dir))
            {
                LogClimbableDebug("Climbing to other climbable in corner");

                Vector3 directionToClimbable = ((Vector3)Block.Position - climbablePos).normalized;
                Move((directionToClimbable * ClimbableOffset) + ((Vector3)dir * ClimbableOffset));
                SetClimbable(target);
                LookAt(dir);
            }
            else
            {
                Vector3 directionToClimbable = ((Vector3)Block.Position - climbablePos).normalized;
                LogClimbableDebug("Stepping off climbable sideways");
                if (target != null && !target.IsSolid && !target.HasFaceAt((-dir).ToDirection()))
                {
                    Move(dir + (directionToClimbable * ClimbableOffset));
                    ResetClimbable();
                }
                else if (movable.TryMove(dir))
                {
                    Move(dir + (directionToClimbable * ClimbableOffset));
                    ResetClimbable();
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
                Gizmos.DrawWireCube(OnClimbable.Position, Vector3.one);
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

        class HeroState : PersistableState
        {
            public Block onClimbable;
            public bool isAlive;
        }

        public PersistableState GetState()
        {
            return new HeroState
            {
                onClimbable = OnClimbable,
                isAlive = IsAlive
            };
        }

        public void ApplyState(PersistableState persistableState)
        {
            var state = persistableState.As<HeroState>();
            IsAlive = state.isAlive;
            SetClimbable(state.onClimbable);
        }
    }
}