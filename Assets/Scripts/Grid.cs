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

    public static bool IsEmpty(Vector3Int pos)
    {
        return !Blocks.ContainsKey(pos);
    }
}