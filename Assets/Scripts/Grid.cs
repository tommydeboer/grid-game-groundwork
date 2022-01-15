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
        var levelTransform = GameObject.FindWithTag("Level").transform;

        foreach (Transform item in levelTransform)
        {
            var block = item.GetComponentInParent<Block>();
            Blocks[block.Tile.gridPos] = block;
        }
    }

    public static T Get<T>(Vector3Int pos) where T : Block
    {
        if (Blocks.ContainsKey(pos) && Blocks[pos] is T t)
        {
            return t;
        }

        return null;
    }

    public static bool Has<T>(Vector3Int pos) where T : Block
    {
        return Get<T>(pos) != null;
    }

    public static bool HasOriented<T>(Vector3Int pos, Vector3Int orientation) where T : Block
    {
        var block = Get<T>(pos);
        return block != null && block.Orientation == orientation;
    }

    public static bool IsEmpty(Vector3Int pos)
    {
        return !Blocks.ContainsKey(pos);
    }
}