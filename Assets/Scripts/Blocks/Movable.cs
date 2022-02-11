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

        [HideInInspector]
        public Vector3 goalPosition;

        [HideInInspector]
        public bool isFalling;

        Grid grid;
        Hero hero;

        void Start()
        {
            grid = CoreComponents.Grid;
            hero = GetComponent<Hero>();
        }

        public void Reset()
        {
            isFalling = false;
        }

        public bool TryMove(Vector3Int dir)
        {
            Vector3Int posToCheck = Block.Tile.gridPos + dir;
            if (grid.Has<Static>(posToCheck))
            {
                return false;
            }

            if (!TryPush(dir, posToCheck)) return false;

            if (hero == null)
            {
                TryMoveStacked(dir);
            }

            return true;
        }

        void TryMoveStacked(Vector3Int dir)
        {
            Vector3Int above = Block.Tile.gridPos + Vector3Int.up;
            Movable stackedMover = grid.Get<Movable>(above);
            if (stackedMover != null)
            {
                if (stackedMover.TryMove(dir))
                {
                    stackedMover.ScheduleMove(dir);
                }
            }
        }

        bool TryPush(Vector3Int dir, Vector3Int posToCheck)
        {
            Movable m = grid.Get<Movable>(posToCheck);
            if (m != null && m != this)
            {
                if (!Game.isPolyban)
                {
                    return false;
                }

                if (m.TryMove(dir))
                {
                    m.ScheduleMove(dir);
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
            if (hero != null)
            {
                if (hero.OnClimbable)
                {
                    return false;
                }

                if (grid.Has<Container>(Block.Below))
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

                goalPosition = transform.position + Vector3.down;
                transform.DOMove(goalPosition, Game.instance.fallTime).OnComplete(FallAgain).SetEase(Ease.Linear);
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

                isFalling = false;
                Game.instance.movingCount--;
                Game.instance.FallEnd();
            }
        }

        bool GroundBelowTile()
        {
            if (grid.Has<Static>(Block.Below))
            {
                return true;
            }

            if (grid.Has<Movable>(Block.Below))
            {
                // Need to get all movers because an Hero can be inside a Container
                List<Movable> movables = grid.GetAll<Movable>(Block.Below);
                return movables.Any(movable => !movable.isFalling);
            }

            return false;
        }
    }
}