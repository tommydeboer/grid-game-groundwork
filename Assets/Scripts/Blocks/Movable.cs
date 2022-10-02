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
        FMODUnity.EventReference LandedEvent;

        [SerializeField]
        FMODUnity.EventReference MovingEvent;

        [HideInInspector]
        public bool isFalling;

        bool isMoving;
        Vector3 previousPos;

        ParticleSystem particleSys;
        AnimationEventListener animationEventListener;

        bool IsMoving
        {
            set
            {
                if (value == isMoving || MovingEvent.IsNull) return;
                if (value)
                {
                    sfxMoving.start();
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(sfxMoving, GetComponent<Transform>());
                }
                else
                {
                    sfxMoving.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }

                isMoving = value;
            }
        }

        FMOD.Studio.EventInstance sfxMoving;
        Removable removable;

        void Start()
        {
            animationEventListener = GetComponentInChildren<AnimationEventListener>();
            particleSys = GetComponent<ParticleSystem>();
            removable = GetComponent<Removable>();

            if (!MovingEvent.IsNull)
            {
                sfxMoving = FMODUnity.RuntimeManager.CreateInstance(MovingEvent);
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(sfxMoving, GetComponent<Transform>());
            }

            previousPos = transform.position;
        }

        void Update()
        {
            var pos = transform.position;
            IsMoving = !isFalling && pos != previousPos;
            previousPos = pos;
        }

        void OnDestroy()
        {
            sfxMoving.release();
        }

        void OnEnable()
        {
            allMovables.Add(this);
        }

        void OnDisable()
        {
            allMovables.Remove(this);
        }

        public MoveResult TryMove(Direction dir)
        {
            MoveResult result = MoveHandler.TryMove(GridElement, dir);
            if (result.DidMove)
            {
                scheduledMoves.Add(GridAnimationFactory.Create(this, result, animationEventListener));
            }

            return result;
        }

        public MoveResult TryTopple(Direction dir)
        {
            MoveResult result = ToppleHandler.TryTopple(GridElement, dir);
            if (result.DidMove)
            {
                scheduledMoves.Add(GridAnimationFactory.Create(this, result, animationEventListener));
            }

            return result;
        }

        public void Fall()
        {
            if (RemoveBelow0()) return;

            if (FallHandler.ShouldFall(GridElement))
            {
                if (!isFalling)
                {
                    isFalling = true;
                }

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
                FallEnd();
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

        void FallEnd()
        {
            if (isFalling)
            {
                if (!LandedEvent.IsNull)
                {
                    FMODUnity.RuntimeManager.PlayOneShot(LandedEvent, transform.position);
                }

                if (particleSys)
                {
                    particleSys.Play();
                }

                isFalling = false;
            }
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
                isFalling = isFalling
            };
        }

        public void ApplyState(PersistableState persistableState)
        {
            var state = persistableState.As<MovableState>();
            var tf = transform;
            tf.position = state.position;
            tf.eulerAngles = state.rotation;
            isFalling = state.isFalling;
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