using System;
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
        TurnLifecycleEventChannelSO turnLifecycleEventChannel;

        [SerializeField]
        FMODUnity.EventReference WalkEvent;

        public Block OnClimbable { get; set; }
        public const float ClimbableOffset = 0.35f;

        bool InputAllowed { get; set; } = true;

        Vector3Int currentMovementDir;
        Vector3 mountDirection;

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
            turnLifecycleEventChannel.OnInputStart += OnInputStart;
            turnLifecycleEventChannel.OnInputEnd += OnInputEnd;
        }

        void OnDisable()
        {
            crushable.OnCrushed -= OnCrush;
            turnLifecycleEventChannel.OnInputStart -= OnInputStart;
            turnLifecycleEventChannel.OnInputEnd -= OnInputEnd;
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
                TryPlayerMove(currentMovementDir);
                turnLifecycleEventChannel.EndInput();
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
            currentMovementDir = new Vector3Int((int)movement.x, 0, (int)movement.y);

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


        void TryPlayerMove(Vector3Int dir)
        {
            if (OnClimbable)
            {
                if ((dir != Vector3Int.forward && dir != Vector3Int.back) && mountDirection == dir)
                {
                    // prevent climbing when we just mounted a ladder sideways to prevent immediately stepping off
                    return;
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