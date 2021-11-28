using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Mover : MonoBehaviour
{
    [HideInInspector]
    public Vector3 goalPosition;

    public readonly List<Tile> tiles = new();

    [HideInInspector]
    public bool isFalling;

    public bool isPlayer => CompareTag("Player");

    void Start()
    {
        CreateTiles();
    }

    void CreateTiles()
    {
        tiles.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Tile"))
            {
                Tile tile = new Tile {t = child};
                tiles.Add(tile);
            }
        }
    }

    public void Reset()
    {
        isFalling = false;
    }

    protected virtual bool CanMove(Vector3 dir)
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
                if (!isPlayer && !Game.isPolyban)
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

            goalPosition = transform.position + Vector3.forward;
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
        foreach (Tile tile in tiles)
        {
            if (Utils.Roughly(tile.pos.z, 0))
            {
                return true;
            }

            if (GroundBelowTile(tile))
            {
                return true;
            }
        }

        return false;
    }

    bool GroundBelowTile(Tile tile)
    {
        Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + Vector3.forward);
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

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            CreateTiles();
        }

        Gizmos.color = Color.blue;
        foreach (Tile tile in tiles)
        {
            Gizmos.DrawWireCube(tile.pos, Vector3.one);
        }
    }
}