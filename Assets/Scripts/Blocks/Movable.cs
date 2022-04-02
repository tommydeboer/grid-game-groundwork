using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridGame.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GridGame.Blocks
{
    public class Movable : BlockBehaviour
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
        Hero hero;
        Container container;

        void Start()
        {
            hero = GetComponent<Hero>();
            container = GetComponent<Container>();
            particleSys = GetComponent<ParticleSystem>();

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

        public void Reset()
        {
            isFalling = false;
        }

        public bool TryMove(Vector3Int dir)
        {
            var neighbour = Block.GetNeighbour(dir);
            if (neighbour)
            {
                if (!neighbour.Is<Movable>()) return false;
                if (!TryPush(dir, neighbour.GetComponent<Movable>())) return false;
            }

            if (!hero)
            {
                TryMoveStacked(dir);
            }

            return true;
        }

        void TryMoveStacked(Vector3Int dir)
        {
            Movable stackedMovable = Block.GetNeighbouring<Movable>(Vector3Int.up);
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
                if (!Game.isPolyban)
                {
                    return false;
                }

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
            if (hero)
            {
                if (hero.OnClimbable)
                {
                    return false;
                }

                if (Block.HasNeighbouring<Container>(Vector3Int.down))
                {
                    return true;
                }
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

                transform.DOMove(Block.Below, Game.instance.fallTime).OnComplete(FallAgain).SetEase(Ease.Linear);

                if (!container)
                {
                    Block.GetNeighbouring<Crushable>(Vector3Int.down)?.Crush();
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
            if (Block.HasNeighbouring<Static>(Vector3Int.down))
            {
                return true;
            }

            Movable below = Block.GetNeighbouring<Movable>(Vector3Int.down);
            if (below)
            {
                if (below.isFalling) return false;
                if (below.GetComponent<Crushable>()) return false;
                return true;
            }

            return false;
        }
    }
}