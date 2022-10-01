using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GridGame.Blocks;
using GridGame.Blocks.Rules;
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

        [SerializeField]
        Animator modelAnimator;

        public Block OnClimbable { get; set; }
        public bool IsPushing { get; private set; }

        Vector3 mountDirection;

        Movable movable;
        Crushable crushable;
        Removable removable;
        PlayerInputController inputController;
        static readonly int isClimbing = Animator.StringToHash("IsClimbing");

        void Awake()
        {
            movable = GetComponent<Movable>();
            crushable = GetComponent<Crushable>();
            removable = GetComponent<Removable>();
            inputController = GetComponent<PlayerInputController>();
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
            if (CanInput())
            {
                IsPushing = false;
                if (inputController.CurrentMovementDir != Vector3Int.zero)
                {
                    if (IsPushing && !inputController.HasDirectionChanged)
                    {
                        // prevent evaluating the same impossible move every frame
                        return;
                    }

                    var result = TryPlayerMove(inputController.CurrentMovementDir);
                    if (result.DidMove)
                    {
                        IsPushing = result.DidMoveOther;

                        gameLoopEventChannelSo.EndInput();
                    }
                    else
                    {
                        IsPushing = true;
                    }
                }
                else
                {
                    mountDirection = Vector3Int.zero;
                }
            }
        }

        bool CanInput()
        {
            return inputController.InputAllowed && !inputController.HoldingUndo && removable.IsAlive;
        }


        public void LookAt(Vector3 dir)
        {
            var q = Quaternion.LookRotation(dir);
            transform.DORotate(q.eulerAngles, 0.1f);
        }


        MoveResult TryPlayerMove(Vector3Int dir)
        {
            if (OnClimbable)
            {
                if ((dir != Vector3Int.forward && dir != Vector3Int.back) && mountDirection == dir)
                {
                    // prevent climbing when we just mounted a ladder sideways to prevent immediately stepping off
                    return MoveResult.Failed();
                }

                // correct input direction based on climbable's orientation
                dir = Vector3Int.RoundToInt(Quaternion.Euler(OnClimbable.Rotation) * dir);
            }

            var result = movable.TryMove(dir.ToDirection());
            if (result.DidMove) PlayWalkSound();

            if (!OnClimbable)
            {
                LookAt(dir);
            }

            return result;
        }

        void PlayWalkSound()
        {
            FMODUnity.RuntimeManager.PlayOneShot(WalkEvent, transform.position);
        }

        public void Mount(Block climbable, Direction direction)
        {
            mountDirection = direction.AsVector();
            OnClimbable = climbable;
            modelAnimator.SetBool(isClimbing, true);
            LookAt(direction.AsVector());
        }

        public void Dismount()
        {
            modelAnimator.SetBool(isClimbing, false);
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