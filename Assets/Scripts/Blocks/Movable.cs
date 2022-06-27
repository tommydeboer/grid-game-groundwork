using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        GridElement GridElement;

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
            // TODO remove this hack when movable is an interface
            GridElement = GetComponent<Block>();
            if (!GridElement) GridElement = GetComponent<Entity>();

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

        public bool TryMove(Vector3Int dir, Block target = null)
        {
            if (!target) target = GridElement.GetNeighbour(dir);
            if (target)
            {
                if (!target.Is<Movable>()) return false;
                if (!TryPush(dir, target.GetComponent<Movable>())) return false;
            }

            if (GridElement.Is<Block>())
            {
                TryMoveStacked(dir);
            }

            return true;
        }

        void TryMoveStacked(Vector3Int dir)
        {
            Movable stackedMovable = GridElement.GetNeighbouring<Movable>(Vector3Int.up);
            if (stackedMovable)
            {
                if (stackedMovable.TryMove(dir))
                {
                    stackedMovable.ScheduleMove(dir);
                }
            }
        }

        bool TryPush(Vector3Int dir, Movable neighbour)
        {
            if (neighbour != this)
            {
                if (neighbour.TryMove(dir))
                {
                    neighbour.ScheduleMove(dir);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void ScheduleMove(Vector3 dir)
        {
            if (!Game.moversToMove.Contains(this))
            {
                goalPosition = transform.position + dir;
                Game.moversToMove.Add(this);
            }
        }

        bool ShouldFall()
        {
            if (GridElement.AttachedTo)
            {
                return false;
            }

            if (GroundBelowTile())
            {
                return false;
            }

            return true;
        }

        public void FallStart()
        {
            if (ShouldFall())
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

        bool GroundBelowTile()
        {
            Block below = GridElement.GetNeighbour(Direction.Down.AsVector());
            if (!below) return false;

            Movable movableBelow = below.GetComponent<Movable>();
            if (movableBelow && movableBelow.isFalling)
            {
                return false;
            }

            if (GridElement.Is<Block>())
            {
                return below.Is<Block>();
            }
            else if (below.IsSolid || below.HasFaceAt(Direction.Up))
            {
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