using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PositionBuffer
{
    static Dictionary<Vector3Int, List<Block>> blocks;

    static Dictionary<Vector3Int, List<Block>> Blocks
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
        Blocks = new Dictionary<Vector3Int, List<Block>>();
        var levelTransform = GameObject.Find("Levels").transform.GetChild(0);

        foreach (Transform item in levelTransform)
        {
            foreach (var tileItem in item.GetComponentsInChildren<BoxCollider>())
            {
                var tilePos = Utils.Vec3ToInt(tileItem.transform.position);

                if (!Blocks.ContainsKey(tilePos))
                {
                    var list = new List<Block>();
                    Blocks.Add(tilePos, list);
                }

                Blocks[tilePos].Add(item.GetComponentInParent<Block>());
            }
        }
    }

    static List<Block> Get(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos)) return Blocks[pos];
        return null;
    }


    static Wall GetWallAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            foreach (Block block in blocks[pos])
            {
                if (block is Wall wall) return wall;
            }
        }

        return null;
    }

    public static bool IsEmpty(Vector3Int pos)
    {
        return !Blocks.ContainsKey(pos);
    }


    public static bool WallIsAtPos(Vector3Int pos)
    {
        return GetWallAtPos(pos) != null;
    }

    public static bool LadderIsAtPos(Vector3Int pos)
    {
        return GetLadderAtPos(pos) != null;
    }

    public static bool MoverIsAtPos(Vector3Int pos)
    {
        return GetMoverAtPos(pos) != null;
    }

    public static Ladder GetLadderAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            foreach (Block block in blocks[pos])
            {
                if (block is Ladder ladder) return ladder;
            }
        }

        return null;
    }

    // MOVERS // 

    public static Mover GetMoverAtPos(Vector3Int pos)
    {
        if (Blocks.ContainsKey(pos))
        {
            foreach (Block block in blocks[pos])
            {
                if (block is Mover mover) return mover;
            }
        }

        return null;
    }
}