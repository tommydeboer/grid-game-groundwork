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

    protected bool CanMove(Vector3 dir)
    {
        foreach (Tile tile in tiles)
        {
            Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + dir);
            if (PositionBuffer.WallIsAtPos(posToCheck))
            {
                return false;
            }

            Mover m = PositionBuffer.GetMoverAtPos(posToCheck);
            if (m != null && m != this)
            {
                if (Type != BlockType.Player && !Game.isPolyban)
                {
                    return false;
                }

                if (m.CanMove(dir))
                {
                    m.MoveIt(dir);
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    protected void MoveIt(Vector3 dir)
    {
        if (!Game.moversToMove.Contains(this))
        {
            goalPosition = transform.position + dir;
            Game.moversToMove.Add(this);
        }
    }

    bool ShouldFall()
    {
        if (GroundBelow())
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

    bool GroundBelow()
    {
        return tiles.Any(GroundBelowTile);
    }

    bool GroundBelowTile(Tile tile)
    {
        Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + Vector3.down);
        if (PositionBuffer.WallIsAtPos(posToCheck))
        {
            return true;
        }

        Mover m = PositionBuffer.GetMoverAtPos(posToCheck);
        if (m != null && m != this && !m.isFalling)
        {
            return true;
        }

        return false;
    }
}