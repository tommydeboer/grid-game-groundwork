using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Blocks.Rules;
using GridGame.Grid;
using GridGame.Player;
using GridGame.SO;
using GridGame.Undo;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Object = System.Object;

namespace GridGame.Blocks
{
    public class Movable : GridBehaviour, IUndoable, IRemovable
    {
        [SerializeField]
        GridAnimationCollection scheduledMoves;

        [SerializeField]
        MovableCollection allMovables;

        [SerializeField]
        GameLoopEventChannelSO gameLoopEventChannel;

        public UnityAction<bool> OnFallingChanged;
        public UnityAction<MovableEventType> OnMovableEvent;

        bool isFalling;

        public bool IsFalling
        {
            get => isFalling;
            private set
            {
                // TODO use MovableEvent instead of OnFallingChanged (used by Hero)
                OnFallingChanged?.Invoke(value);

                if (isFalling && !value) OnMovableEvent?.Invoke(MovableEventType.LANDED_FALL);
                isFalling = value;
            }
        }

        bool isToppling;

        bool IsToppling
        {
            get => isToppling;
            set
            {
                if (isToppling && !value && GridElement.IsGrounded())
                {
                    OnMovableEvent?.Invoke(MovableEventType.LANDED_TOPPLE);
                }

                if (!isToppling && value)
                {
                    OnMovableEvent?.Invoke(MovableEventType.START_TOPPLE);
                }

                isToppling = value;
            }
        }

        bool isSliding;

        bool IsSliding
        {
            set
            {
                if (!isSliding && value) OnMovableEvent?.Invoke(MovableEventType.START_SLIDE);
                if (isSliding && !value) OnMovableEvent?.Invoke(MovableEventType.STOP_SLIDE);
                isSliding = value;
            }
        }

        AnimationEventListener animationEventListener;

        Removable removable;

        void Start()
        {
            animationEventListener = GetComponentInChildren<AnimationEventListener>();
            removable = GetComponent<Removable>();
        }

        void OnEnable()
        {
            allMovables.Add(this);
            gameLoopEventChannel.OnFallStart += OnFallStart;
        }

        void OnDisable()
        {
            allMovables.Remove(this);
            gameLoopEventChannel.OnFallStart -= OnFallStart;
        }

        void OnFallStart()
        {
            IsSliding = false;
            IsToppling = false;
        }

        public MoveResult TryMove(Direction dir)
        {
            MoveResult result = MoveHandler.TryMove(GridElement, dir);
            if (result.DidMove)
            {
                IsSliding = true;
                scheduledMoves.Add(GridAnimationFactory.Create(this, result, animationEventListener));
            }

            return result;
        }

        public MoveResult TryTopple(Direction dir)
        {
            MoveResult result = ToppleHandler.TryTopple(GridElement, dir);
            if (result.DidMove)
            {
                IsToppling = true;
                scheduledMoves.Add(GridAnimationFactory.Create(this, result, animationEventListener));
            }

            return result;
        }

        public void Fall()
        {
            if (RemoveBelow0()) return;

            if (FallHandler.ShouldFall(GridElement))
            {
                IsFalling = true;

                scheduledMoves.Add(new LinearAnimation
                {
                    Movable = this,
                    TargetPosition = GridElement.Below
                });

                // TODO FIXME remove and use collisions to crush instead
                // TODO crushing should be a Rule?
                Block block = GetComponent<Block>();
                if (block && (block.IsSolid || block.HasFaceAt(Direction.Down)))
                {
                    GridElement.GetNeighbouring<Crushable>(Direction.Down)?.Crush();
                }
            }
            else
            {
                IsFalling = false;
            }
        }

        bool RemoveBelow0()
        {
            if (transform.position.y < 0)
            {
                removable.Remove();
                return true;
            }

            return false;
        }

        class MovableState : PersistableState
        {
            public Vector3 position;
            public Vector3 rotation;
            public bool isFalling;
        }

        public PersistableState GetState()
        {
            var tf = transform;
            return new MovableState
            {
                position = tf.position,
                rotation = tf.eulerAngles,
                isFalling = IsFalling
            };
        }

        public void ApplyState(PersistableState persistableState)
        {
            var state = persistableState.As<MovableState>();
            var tf = transform;
            tf.position = state.position;
            tf.eulerAngles = state.rotation;
            IsFalling = state.isFalling;
        }

        public void OnRemove()
        {
            allMovables.Remove(this);
            enabled = false;
        }

        public void OnReplace()
        {
            allMovables.Add(this);
            enabled = true;
        }
    }
}