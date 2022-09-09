using System;
using System.Collections;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.SO;
using GridGame.Undo;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Player
{
    [RequireComponent(typeof(Movable))]
    public class Hero : Entity, IUndoable
    {
        [SerializeField]
        GameLoopEventChannelSO gameLoopEventChannelSo;

        [SerializeField]
        FMODUnity.EventReference WalkEvent;

        public Block OnClimbable { get; set; }
        public const float ClimbableOffset = 0.35f;
        public bool IsBlocked { get; private set; }

        bool InputAllowed { get; set; } = true;

        Vector3Int currentMovementDir;
        Vector3 mountDirection;
        bool hasDirectionChanged;

        Movable movable;
        Crushable crushable;
        Removable removable;
        bool holdingUndo;

        void Awake()
        {
            movable = GetComponent<Movable>();
            crushable = GetComponent<Crushable>();
            removable = GetComponent<Removable>();
        }

        void OnEnable()
        {
            crushable.OnCrushed += OnCrush;
            gameLoopEventChannelSo.OnInputStart += OnInputStart;
            gameLoopEventChannelSo.OnInputEnd += OnInputEnd;
        }

        void OnDisable()
        {
            crushable.OnCrushed -= OnCrush;
            gameLoopEventChannelSo.OnInputStart -= OnInputStart;
            gameLoopEventChannelSo.OnInputEnd -= OnInputEnd;
        }

        void OnInputStart() => InputAllowed = true;
        void OnInputEnd() => InputAllowed = false;

        void OnCrush()
        {
            removable.Remove();
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
                if (IsBlocked && !hasDirectionChanged)
                {
                    // prevent evaluating the same impossible move every frame
                    return;
                }

                if (TryPlayerMove(currentMovementDir))
                {
                    gameLoopEventChannelSo.EndInput();
                }
                else
                {
                    IsBlocked = true;
                }
            }
            else
            {
                IsBlocked = false;
            }
        }

        bool CanInput()
        {
            return InputAllowed && !holdingUndo && removable.IsAlive;
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
            var dir = new Vector3Int((int)movement.x, 0, (int)movement.y);

            hasDirectionChanged = dir != currentMovementDir;
            currentMovementDir = dir;

            if (currentMovementDir == Vector3Int.zero)
            {
                mountDirection = Vector3Int.zero;
            }
        }

        public void LookAt(Vector3 dir)
        {
            var q = Quaternion.LookRotation(dir);
            transform.DORotate(q.eulerAngles, 0.1f);
        }


        bool TryPlayerMove(Vector3Int dir)
        {
            if (OnClimbable)
            {
                if ((dir != Vector3Int.forward && dir != Vector3Int.back) && mountDirection == dir)
                {
                    // prevent climbing when we just mounted a ladder sideways to prevent immediately stepping off
                    return false;
                }

                // correct input direction based on climbable's orientation
                dir = Vector3Int.RoundToInt(Quaternion.Euler(OnClimbable.Rotation) * dir);
            }

            bool didMove = movable.TryMove(dir.ToDirection());
            if (didMove) PlayWalkSound();

            if (!OnClimbable)
            {
                LookAt(dir);
            }

            return didMove;
        }

        void PlayWalkSound()
        {
            FMODUnity.RuntimeManager.PlayOneShot(WalkEvent, transform.position);
        }

        public void Mount(Block climbable, Direction direction)
        {
            mountDirection = direction.AsVector();
            OnClimbable = climbable;
            LookAt(direction.AsVector());
        }

        public void Dismount()
        {
            OnClimbable = null;
        }

        public void OnDrawGizmos()
        {
            if (OnClimbable)
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

        class HeroState : PersistableState
        {
            public Block onClimbable;
        }

        public PersistableState GetState()
        {
            return new HeroState
            {
                onClimbable = OnClimbable,
            };
        }

        public void ApplyState(PersistableState persistableState)
        {
            var state = persistableState.As<HeroState>();
            OnClimbable = state.onClimbable;
        }
    }
}