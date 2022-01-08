using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Grid
{
    static Dictionary<Vector3Int, Block> blocks;

    static Dictionary<Vector3Int, Block> Blocks
    {
        get
        {
            if (blocks == null) Update();
            return blocks;
        }
        set => blocks = value;
    }

    public static void Update()
    {
        Blocks = new Dictionary<Vector3Int, Block>();
        var levelTransform = GameObject.Find("Levels").transform.GetChild(0);

        foreach (Transform item in levelTransform)
        {
            foreach (var tileItem in item.GetComponentsInChildren<BoxCollider>())
            {
                var tilePos = Utils.Vec3ToInt(tileItem.transform.position);

                Blocks[tilePos] = item.GetComponentInParent<Block>();
            }
        }
    }

    static Block Get(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos)) return Blocks[pos];
        return null;
    }


    static Wall GetWallAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            if (Blocks[pos] is Wall wall) return wall;
        }

        return null;
    }

    public static bool IsEmpty(Vector3Int pos)
    {
        return !Blocks.ContainsKey(pos);
    }


    public static bool HasWallAtPos(Vector3Int pos)
    {
        return GetWallAtPos(pos) != null;
    }

    public static bool HasLadderAtPos(Vector3Int pos)
    {
        return GetLadderAtPos(pos) != null;
    }

    public static bool HasMoverAtPos(Vector3Int pos)
    {
        return GetMoverAtPos(pos) != null;
    }

    public static Ladder GetLadderAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            if (Blocks[pos] is Ladder ladder) return ladder;
        }

        return null;
    }

    public static Mover GetMoverAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            if (Blocks[pos] is Mover mover) return mover;
        }

        return null;
    }
}