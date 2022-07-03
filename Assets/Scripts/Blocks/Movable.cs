using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Blocks.Interactions;
using GridGame.Player;
using GridGame.SO;
using GridGame.Undo;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Object = System.Object;

namespace GridGame.Blocks
{
    public class Movable : GridBehaviour, IUndoable
    {
        [SerializeField]
        FMODUnity.EventReference LandedEvent;

        [SerializeField]
        FMODUnity.EventReference MovingEvent;

        [HideInInspector]
        public Vector3 goalPosition;

        [HideInInspector]
        public bool isFalling;

        bool isMoving;
        Vector3 previousPos;

        ParticleSystem particleSys;

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
        Game game;

        void Start()
        {
            game = CoreComponents.Game;
            particleSys = GetComponent<ParticleSystem>();

            game.RegisterMovable(this);

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
            game.UnregisterMovable(this);
        }

        public bool TryMove(Vector3Int dir)
        {
            MoveResult result = MoveHandler.TryMove(GridElement, dir.ToDirection());
            if (result.DidMove)
            {
                ScheduleMove(dir);
                MoveHandler.TryMoveStacked(GridElement, dir.ToDirection());
            }

            return result.DidMove;
        }

        public void ScheduleMove(Vector3 dir)
        {
            if (!Game.moversToMove.Contains(this))
            {
                goalPosition = transform.position + dir;
                Game.moversToMove.Add(this);
            }
        }

        public void FallStart()
        {
            if (FallHandler.ShouldFall(GridElement))
            {
                if (!isFalling)
                {
                    isFalling = true;
                    Game.instance.movingCount++;
                }

                transform.DOMove(GridElement.Below, Game.instance.fallTime).OnComplete(FallAgain).SetEase(Ease.Linear);

                // TODO FIXME remove and use collisions to crush instead
                Block block = GetComponent<Block>();
                if (block && (block.IsSolid || block.HasFaceAt(Direction.Down)))
                {
                    GridElement.GetNeighbouring<Crushable>(Vector3Int.down)?.Crush();
                }
            }
            else
            {
                FallEnd();
            }
        }

        void FallAgain()
        {
            StartCoroutine(DoFallAgain());
        }

        IEnumerator DoFallAgain()
        {
            yield return WaitFor.EndOfFrame;
            FallStart();
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
                Game.instance.movingCount--;
                Game.instance.FallEnd();
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
    }
}