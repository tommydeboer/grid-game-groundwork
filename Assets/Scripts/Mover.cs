using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Mover : Block
{
    [HideInInspector]
    public Vector3 goalPosition;

    [HideInInspector]
    public bool isFalling;

    public override BlockType Type => BlockType.Mover;

    public void Reset()
    {
        isFalling = false;
    }

    protected bool TryMove(Vector3Int dir)
    {
        Vector3Int posToCheck = Tile.gridPos + dir;

        if (Grid.Has<Wall>(posToCheck))
        {
            return false;
        }

        if (!TryPush(dir, posToCheck)) return false;

        TryMoveStacked(dir);

        return true;
    }

    void TryMoveStacked(Vector3Int dir)
    {
        Vector3Int above = Tile.gridPos + Vector3Int.up;
        Mover stackedMover = Grid.Get<Mover>(above);
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
        Mover m = Grid.Get<Mover>(posToCheck);
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

    protected void ScheduleMove(Vector3 dir)
    {
        if (!Game.moversToMove.Contains(this))
        {
            goalPosition = transform.position + dir;
            Game.moversToMove.Add(this);
        }
    }

    protected virtual bool ShouldFall()
    {
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
            isFalling = false;
            Game.instance.movingCount--;
            Game.instance.FallEnd();
        }
    }

    bool GroundBelowTile()
    {
        Vector3Int posToCheck = Tile.gridPos + Vector3Int.down;
        if (Grid.Has<Wall>(posToCheck))
        {
            return true;
        }

        Mover m = Grid.Get<Mover>(posToCheck);
        if (m != null && m != this && !m.isFalling)
        {
            return true;
        }

        return false;
    }
}